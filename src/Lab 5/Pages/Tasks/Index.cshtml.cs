using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using RazorPagesHtmxWorkshop.Data;
using RazorPagesHtmxWorkshop.Models;

namespace RazorPagesHtmxWorkshop.Pages.Tasks;

public class IndexModel : PageModel
{
    public IReadOnlyList<TaskItem> Tasks { get; private set; } = Array.Empty<TaskItem>();

    [BindProperty]
    public NewTaskInput Input { get; set; } = new();

    [TempData]
    public string? FlashMessage { get; set; }

    // Filter/pagination state for initial render
    public string? Query { get; set; }
    public int CurrentPage { get; set; } = 1;
    public int PageSize { get; set; } = 5;
    public int TotalTasks { get; set; }

    #region Helper Methods

    /// <summary>
    /// Checks if the current request was made by htmx.
    /// htmx sends "HX-Request: true" header with every request.
    /// </summary>
    private bool IsHtmx() =>
        Request.Headers.TryGetValue("HX-Request", out var value) && value == "true";

    /// <summary>
    /// Returns a partial view result for fragment responses.
    /// This helper creates a PartialViewResult with the correct ViewData context.
    /// </summary>
    private PartialViewResult Fragment(string partialName, object model) =>
        new()
        {
            ViewName = partialName,
            ViewData = new ViewDataDictionary(ViewData) { Model = model }
        };

    #endregion

    #region Page Handlers

    public void OnGet(string? q, int page = 1, int pageSize = 5)
    {
        Query = q;
        CurrentPage = Math.Max(1, page);
        PageSize = Math.Clamp(pageSize, 1, 50);

        var all = InMemoryTaskStore.All();

        if (!string.IsNullOrWhiteSpace(q))
        {
            all = all
                .Where(t => t.Title.Contains(q, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        TotalTasks = all.Count;
        Tasks = all
            .Skip((CurrentPage - 1) * PageSize)
            .Take(PageSize)
            .ToList();
    }

    /// <summary>
    /// Returns the task list fragment with optional filtering and pagination.
    /// Supports query parameter (q) for filtering and page/pageSize for pagination.
    /// </summary>
    public IActionResult OnGetList(string? q, int page = 1, int pageSize = 5)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 50);

        var all = InMemoryTaskStore.All();

        if (!string.IsNullOrWhiteSpace(q))
        {
            all = all
                .Where(t => t.Title.Contains(q, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        var total = all.Count;
        var items = all
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        var vm = new TaskListVm
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            Total = total,
            Query = q
        };

        return Fragment("Partials/_TaskList", vm);
    }

    /// <summary>
    /// Returns the details fragment for a specific task.
    /// Called via hx-get from the Details button in each row.
    /// </summary>
    public IActionResult OnGetDetails(int id)
    {
        var task = InMemoryTaskStore.Find(id);
        return Fragment("Partials/_TaskDetails", task);
    }

    /// <summary>
    /// Returns the messages fragment.
    /// Called by htmx listener when showMessage event fires.
    /// </summary>
    public IActionResult OnGetMessages()
    {
        return Fragment("Partials/_Messages", FlashMessage);
    }

    /// <summary>
    /// Returns a reset/empty form fragment.
    /// Called by htmx listener when clearForm event fires.
    /// </summary>
    public IActionResult OnGetEmptyForm()
    {
        Input = new NewTaskInput();
        ModelState.Clear();
        return Fragment("Partials/_TaskForm", this);
    }

    #endregion

    #region Validation Handlers

    /// <summary>
    /// Validates the Title field and returns just the validation fragment.
    /// Called via htmx on keystrokes (debounced).
    /// </summary>
    public IActionResult OnPostValidateTitle()
    {
        var title = Input.Title?.Trim() ?? "";
        string? error = null;

        if (string.IsNullOrWhiteSpace(title))
            error = "Title is required.";
        else if (title.Length < 3)
            error = "Title must be at least 3 characters.";
        else if (title.Length > 60)
            error = "Title must be 60 characters or fewer.";

        return Fragment("Partials/_TitleValidation", error);
    }

    #endregion

    #region Dynamic Tags

    /// <summary>
    /// Returns a new tag row fragment.
    /// The nextIndex parameter ensures unique name attributes for model binding.
    /// </summary>
    public IActionResult OnGetAddTag(int nextIndex)
    {
        return Fragment("Partials/_TagRow", (nextIndex, ""));
    }

    /// <summary>
    /// Handles tag removal. Returns empty response for hx-swap="delete".
    /// </summary>
    public IActionResult OnGetRemoveTag(int index)
    {
        return new EmptyResult();
    }

    #endregion

    #region Dependent Dropdowns

    /// <summary>
    /// Returns the subcategory dropdown options based on selected category.
    /// Called when category dropdown changes.
    /// </summary>
    public IActionResult OnGetSubcategories(string? category)
    {
        var subcategories = CategoryData.GetSubcategories(category);
        return Fragment("Partials/_SubcategorySelect", (subcategories, (string?)null));
    }

    #endregion

    #region Long-Running Jobs

    /// <summary>
    /// Starts a new simulated background job.
    /// Returns the initial status fragment which begins polling.
    /// </summary>
    public IActionResult OnPostStartJob()
    {
        var job = JobSimulator.StartJob();
        return Fragment("Partials/_JobStatus", job);
    }

    /// <summary>
    /// Returns the current status of a job.
    /// Called by polling requests.
    /// When job completes, includes OOB swap for messages.
    /// </summary>
    public IActionResult OnGetJobStatus(string jobId)
    {
        var status = JobSimulator.GetStatus(jobId);
        
        if (status is null)
        {
            return Fragment("Partials/_JobStatus", (JobSimulator.JobStatus?)null);
        }
        
        // For completed or failed jobs, use OOB partial
        if (status.State is "completed" or "failed")
        {
            var message = status.State == "completed"
                ? "Report generation completed successfully!"
                : $"Report generation failed: {status.Error}";
            
            var alertClass = status.State == "completed" ? "success" : "danger";
            
            return Fragment("Partials/_JobStatusWithOob", (status, message, alertClass));
        }
        
        // Running jobs use simple status fragment
        return Fragment("Partials/_JobStatus", status);
    }

    /// <summary>
    /// Resets the job UI to initial state.
    /// </summary>
    public IActionResult OnGetResetJob()
    {
        return Fragment("Partials/_JobStatus", (JobSimulator.JobStatus?)null);
    }

    #endregion

    #region Action Handlers

    public IActionResult OnPostCreate()
    {
        // Clean up empty tags before validation
        Input.Tags = Input.Tags
            .Where(t => !string.IsNullOrWhiteSpace(t))
            .Select(t => t.Trim())
            .ToList();

        if (!TryValidateModel(Input, nameof(Input)))
        {
            Tasks = InMemoryTaskStore.All();

            if (IsHtmx())
            {
                Response.Headers["HX-Retarget"] = "#task-form";
                Response.Headers["HX-Reswap"] = "outerHTML";
                return Fragment("Partials/_TaskForm", this);
            }

            return Page();
        }

        if (Input.Title.Trim().Equals("boom", StringComparison.OrdinalIgnoreCase))
        {
            if (IsHtmx())
            {
                Response.Headers["HX-Retarget"] = "#messages";
                Response.Headers["HX-Reswap"] = "innerHTML";
                return Fragment("Partials/_Error",
                    "Simulated server error. Try a different title.");
            }

            throw new InvalidOperationException("Simulated server error.");
        }

        // Add task with tags
        var task = InMemoryTaskStore.Add(Input.Title);
        
        // Store tags (you'd save these in a real app)
        // For the workshop, we'll just log them
        if (Input.Tags.Count > 0)
        {
            // In a real app: taskTagService.AddTags(task.Id, Input.Tags);
            Console.WriteLine($"Task {task.Id} created with tags: {string.Join(", ", Input.Tags)}");
        }

        // Log category/subcategory if selected
        if (!string.IsNullOrWhiteSpace(Input.Category))
        {
            Console.WriteLine($"Task {task.Id} category: {Input.Category} / {Input.Subcategory}");
        }

        if (IsHtmx())
        {
            var tagCount = Input.Tags.Count;
            FlashMessage = tagCount > 0 
                ? $"Task added with {tagCount} tag(s)!"
                : "Task added successfully!";
            
            // Reset input including tags for the form refresh
            Input = new NewTaskInput();
            
            Response.Headers["HX-Trigger"] = "showMessage,clearForm";

            var all = InMemoryTaskStore.All();
            var vm = new TaskListVm
            {
                Items = all.Take(5).ToList(),
                Page = 1,
                PageSize = 5,
                Total = all.Count,
                Query = null
            };

            return Fragment("Partials/_TaskList", vm);
        }

        FlashMessage = "Task added.";
        return RedirectToPage();
    }

    /// <summary>
    /// Deletes a task and returns the updated list fragment.
    /// Uses hx-confirm on the client for confirmation.
    /// </summary>
    public IActionResult OnPostDelete(int id)
    {
        var removed = InMemoryTaskStore.Delete(id);
        Tasks = InMemoryTaskStore.All();

        if (IsHtmx())
        {
            if (!removed)
            {
                Response.Headers["HX-Retarget"] = "#messages";
                Response.Headers["HX-Reswap"] = "outerHTML";
                return Fragment("Partials/_Messages", "Task not found (already deleted?).");
            }

            FlashMessage = "Task deleted.";
            Response.Headers["HX-Trigger"] = "showMessage";

            var all = InMemoryTaskStore.All();
            var vm = new TaskListVm
            {
                Items = all.Take(5).ToList(),
                Page = 1,
                PageSize = 5,
                Total = all.Count,
                Query = null
            };

            return Fragment("Partials/_TaskList", vm);
        }

        FlashMessage = removed ? "Task deleted." : "Task not found.";
        return RedirectToPage();
    }

    public IActionResult OnPostReset()
    {
        InMemoryTaskStore.Reset();
        FlashMessage = "Tasks reset.";
        return RedirectToPage();
    }

    #endregion

    #region Input Models

    public class NewTaskInput
    {
        [Required(ErrorMessage = "Title is required.")]
        [StringLength(60, MinimumLength = 3, ErrorMessage = "Title must be 3–60 characters.")]
        public string Title { get; set; } = "";

        /// <summary>
        /// Tags for the task. Each tag is a simple string.
        /// Model binding uses index notation: Tags[0], Tags[1], etc.
        /// </summary>
        public List<string> Tags { get; set; } = new();

        /// <summary>
        /// Category for the task (first-level selection).
        /// </summary>
        public string? Category { get; set; }

        /// <summary>
        /// Subcategory for the task (depends on Category).
        /// </summary>
        public string? Subcategory { get; set; }
    }

    #endregion
}
