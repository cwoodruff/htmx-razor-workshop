using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
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
            ViewData = new ViewDataDictionary(MetadataProvider, ModelState) { Model = model }
        };

    #endregion

    #region Page Handlers

    public void OnGet()
    {
        Tasks = InMemoryTaskStore.All();
    }

    /// <summary>
    /// Returns the task list fragment.
    /// Optional parameter 'take' limits the number of tasks returned.
    /// </summary>
    public IActionResult OnGetList(int? take)
    {
        var tasks = InMemoryTaskStore.All();

        if (take is > 0)
        {
            tasks = tasks.Take(take.Value).ToList();
        }

        return Fragment("Partials/_TaskList", tasks);
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
    /// 
    /// Design: This handler is intentionally "micro"—one field, one fragment.
    /// It avoids returning the entire form on each keystroke.
    /// </summary>
    public IActionResult OnPostValidateTitle()
    {
        var title = Input.Title?.Trim() ?? "";

        string? error = null;

        if (string.IsNullOrWhiteSpace(title))
        {
            error = "Title is required.";
        }
        else if (title.Length < 3)
        {
            error = "Title must be at least 3 characters.";
        }
        else if (title.Length > 60)
        {
            error = "Title must be 60 characters or fewer.";
        }

        return Fragment("Partials/_TitleValidation", error);
    }

    #endregion

    #region Action Handlers

    public IActionResult OnPostCreate()
    {
        // Validate using data annotations
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

        // Simulated error for testing
        // Type "boom" as the title to trigger this error
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

        // Success
        InMemoryTaskStore.Add(Input.Title);
        Tasks = InMemoryTaskStore.All();

        if (IsHtmx())
        {
            FlashMessage = "Task added successfully!";
            // Trigger events for listeners to handle
            // Multiple events separated by commas
            Response.Headers["HX-Trigger"] = "showMessage,clearForm";
            return Fragment("Partials/_TaskList", Tasks);
        }

        FlashMessage = "Task added.";
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
    }

    #endregion
}
