---
order: 1
icon: code-square
---

# Lab 5: Dynamic Forms + Long-Running UX (Polling)

## Overview

This lab covers two high-value scenarios that traditionally require significant JavaScript but can be elegantly solved with htmx:

1. **Dynamic UI Composition**: Add/remove form rows, dependent dropdowns
2. **Long-Running Operations**: Progress updates via polling without WebSockets

These patterns appear constantly in real applications—order line items, tag management, category filters, background job status, file uploads with progress. By the end of this lab, you'll have reusable blueprints for all of them.

### The Key Insight

Both patterns share a common theme: **the server controls the UI state**.

| Scenario           | Traditional Approach                    | htmx Approach                             |
|--------------------|-----------------------------------------|-------------------------------------------|
| Add form row       | JavaScript clones DOM, manages indexes  | Server renders new row fragment           |
| Dependent dropdown | JavaScript fetches JSON, builds options | Server renders options fragment           |
| Long-running job   | WebSocket or complex polling library    | `hx-trigger="every 1s"` + status endpoint |
| Multiple updates   | Custom event system                     | `hx-swap-oob` for out-of-band swaps       |

---

## Lab Outcomes

By the end of Lab 5, you will be able to:

| Outcome                 | Description                                                       |
|-------------------------|-------------------------------------------------------------------|
| **Add/Remove rows**     | Dynamic sub-collections (tags, line items) via fragment endpoints |
| **Dependent dropdowns** | Cascading selects (Category → Subcategory)                        |
| **Polling**             | Long-running operations with `hx-trigger="every Xs"`              |
| **Out-of-band swaps**   | Update multiple page regions from a single response               |
| **Conditional polling** | Start/stop polling based on job status                            |
| **Enhanced details view** | Display all task properties including tags and categories       |

---

## Prerequisites

Before starting this lab, ensure you have:

- **Completed Lab 4** with all verifications passing
- **Production-livable conventions** in place (checkpoint complete)
- **Working CRUD operations** for Tasks
- **Fragment helpers** (`IsHtmx()` and `Fragment()`) ready

---

## Pattern 1: Dynamic Form Rows (Add/Remove Tags) (15–20 minutes)

This pattern lets users add and remove items in a sub-collection—like tags on a task, line items on an order, or attendees on an event.

### 1.1 The Design

**User Flow:**

1. User sees a task form with a "Tags" section
2. User clicks "Add Tag" → new empty tag input appears
3. User can remove any tag by clicking its "Remove" button (client-side, no server round-trip)
4. On form submit, all tags are collected and saved

**Architecture:**

- Tags are rendered as a list of inputs inside a `#tags-container`
- "Add Tag" button fetches a new tag row fragment from server
- Each tag row has a "Remove" button that removes itself **client-side** using htmx's `hx-on` attribute
- Tags use simple name binding (`Input.Tags`) - ASP.NET Core handles multiple values automatically

### 1.2 Update the TaskItem Model

First, add Tag support to the TaskItem model:

**File: `Models/TaskItem.cs`**

```csharp
namespace RazorPagesHtmxWorkshop.Models;

public class TaskItem
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public bool IsDone { get; set; }
    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;

    public string? Category { get; set; }
    public string? Subcategory { get; set; }
    public List<string> Tags { get; set; } = new();
}
```

### 1.3 Update the Input Model

Extend `NewTaskInput` to support tags:

**File: `Pages/Tasks/Index.cshtml.cs` (update NewTaskInput class)**

```csharp
public class NewTaskInput
{
    [Required(ErrorMessage = "Title is required.")]
    [StringLength(60, MinimumLength = 3, ErrorMessage = "Title must be 3–60 characters.")]
    public string Title { get; set; } = "";

    /// <summary>
    /// Tags for the task. Each tag is a simple string.
    /// Model binding automatically collects multiple inputs with the same name.
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
```

### 1.4 Create the Tag Row Fragment

This fragment renders a single tag input with a client-side remove button:

**File: `Pages/Tasks/Partials/_TagRow.cshtml`**

```cshtml
@model (int Index, string Value)

@*
    Tag Row Fragment
    ================

    Purpose: Single tag input with remove button
    Model: (int Index, string Value) - tuple with index and current value

    Design notes:
    - Uses simple name="Input.Tags" for model binding (no indexes needed!)
    - Remove button uses hx-on:click for client-side removal (no server round-trip)
    - ASP.NET Core model binder handles multiple inputs with same name

    Swap strategy:
    - Added via hx-swap="beforeend" into #tags-list
    - Removed via client-side JavaScript using hx-on:click
*@

<div class="tag-row input-group mb-2">
    <input type="text"
           class="form-control"
           name="Input.Tags"
           value="@Model.Value"
           placeholder="Enter tag..." />

    <button type="button"
            class="btn btn-outline-danger"
            hx-on:click="this.closest('.tag-row').remove()"
            title="Remove tag">
        <span aria-hidden="true">&times;</span>
        <span class="visually-hidden">Remove tag</span>
    </button>
</div>
```

**Key Points:**

- **Simplified binding**: All tag inputs use the same `name="Input.Tags"`, and ASP.NET Core automatically binds them to a List
- **Client-side removal**: Uses `hx-on:click="this.closest('.tag-row').remove()"` to remove the row without a server call
- **No index tracking needed**: The server doesn't need to manage indexes

### 1.5 Create the Tags Container Fragment

This fragment wraps all tag rows and includes the "Add Tag" button:

**File: `Pages/Tasks/Partials/_TagsContainer.cshtml`**

```cshtml
@model List<string>

@*
    Tags Container Fragment
    =======================

    Purpose: Container for all tag rows + Add button
    Model: List<string> - current tags

    Design notes:
    - Container has stable ID #tags-container
    - Add button appends new rows via hx-swap="beforeend"
    - No need to track indexes - model binding handles it
*@

<div id="tags-container" class="mb-3">
    <label class="form-label">Tags</label>

    <div id="tags-list">
        @for (var i = 0; i < Model.Count; i++)
        {
            <partial name="Partials/_TagRow" model="(i, Model[i])" />
        }
    </div>

    <button type="button"
            class="btn btn-sm btn-outline-secondary mt-2"
            hx-get="?handler=AddTag"
            hx-target="#tags-list"
            hx-swap="beforeend">
        <span aria-hidden="true">+</span> Add Tag
    </button>

    <div class="form-text">Add tags to categorize your task.</div>
</div>
```

### 1.6 Add the Tag Handler

**File: `Pages/Tasks/Index.cshtml.cs` (add to #region Dynamic Tags)**

```csharp
#region Dynamic Tags

/// <summary>
/// Returns a new tag row fragment.
/// </summary>
public IActionResult OnGetAddTag()
{
    // Return a new empty tag row (index doesn't matter for binding)
    return Fragment("Partials/_TagRow", (0, ""));
}

/// <summary>
/// Handles tag removal (not actually used - removal is client-side).
/// Kept for potential server-side validation or tracking.
/// </summary>
public IActionResult OnGetRemoveTag()
{
    return new EmptyResult();
}

#endregion
```

### 1.7 Update InMemoryTaskStore

Update the `Add` method to accept tags, category, and subcategory:

**File: `Data/InMemoryTaskStore.cs`**

```csharp
public static TaskItem Add(string title, string? category = null, string? subcategory = null, List<string>? tags = null)
{
    var item = new TaskItem
    {
        Id = _nextId++,
        Title = title.Trim(),
        IsDone = false,
        CreatedUtc = DateTime.UtcNow,
        Category = category,
        Subcategory = subcategory,
        Tags = tags ?? new()
    };

    _tasks.Add(item);
    return item;
}
```

### 1.8 Update OnPostCreate to Handle Tags

**File: `Pages/Tasks/Index.cshtml.cs` (update OnPostCreate method)**

```csharp
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

    // Add task with tags, category and subcategory
    var task = InMemoryTaskStore.Add(Input.Title, Input.Category, Input.Subcategory, Input.Tags);

    // Log details
    if (Input.Tags.Count > 0)
    {
        Console.WriteLine($"Task {task.Id} created with tags: {string.Join(", ", Input.Tags)}");
    }

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

        CurrentPage = Math.Max(1, PageNum);
        PageSize = Math.Clamp(Size, 1, 50);

        var all = InMemoryTaskStore.All();

        if (!string.IsNullOrWhiteSpace(Q))
        {
            all = all
                .Where(t => t.Title.Contains(Q, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        var total = all.Count;
        var items = all
            .Skip((CurrentPage - 1) * PageSize)
            .Take(PageSize)
            .ToList();

        var vm = new TaskListVm
        {
            Items = items,
            Page = CurrentPage,
            PageSize = PageSize,
            Total = total,
            Query = Q
        };

        return Fragment("Partials/_TaskList", vm);
    }

    FlashMessage = "Task added.";
    return RedirectToPage();
}
```

---

## Pattern 2: Dependent Dropdowns (Category → Subcategory) (10–15 minutes)

Dependent dropdowns (cascading selects) update one dropdown based on another's selection—like Category → Subcategory, Country → City, or Make → Model.

### 2.1 The Design

**User Flow:**

1. User selects a Category from the first dropdown
2. Subcategory dropdown updates with relevant options
3. User selects a Subcategory
4. Form can be submitted with both values

**Architecture:**

- Category dropdown has `hx-get` that fetches subcategory options on change
- Subcategory dropdown is wrapped in a swappable container (`#subcategory-container`)
- Server returns the subcategory `<select>` fragment

### 2.2 Create Sample Data

For this example, we'll use simple in-memory data:

**File: `Data/CategoryData.cs` (create new file)**

```csharp
namespace RazorPagesHtmxWorkshop.Data;

/// <summary>
/// Sample data for demonstrating dependent dropdowns.
/// Category → Subcategory cascading selection.
/// </summary>
public static class CategoryData
{
    private static readonly Dictionary<string, List<string>> _subcategories = new()
    {
        ["Work"] = new() { "Meeting", "Report", "Email", "Review" },
        ["Personal"] = new() { "Shopping", "Exercise", "Reading", "Travel" },
        ["Home"] = new() { "Cleaning", "Repairs", "Gardening", "Cooking" },
        ["Learning"] = new() { "Course", "Tutorial", "Practice", "Research" }
    };

    public static IReadOnlyList<string> GetCategories() =>
        _subcategories.Keys.ToList();

    public static IReadOnlyList<string> GetSubcategories(string? category) =>
        string.IsNullOrWhiteSpace(category) || !_subcategories.ContainsKey(category)
            ? Array.Empty<string>()
            : _subcategories[category];
}
```

### 2.3 Create the Subcategory Fragment

**File: `Pages/Tasks/Partials/_SubcategorySelect.cshtml`**

```csharp
@model (IReadOnlyList<string> Options, string? Selected)

@*
    Subcategory Select Fragment
    ===========================

    Purpose: Dropdown for subcategory selection
    Model: (Options list, Selected value)

    Design notes:
    - Wrapper div has stable ID for swapping
    - Select is disabled when no options available
    - Uses Razor syntax for disabled and selected attributes
*@

<div id="subcategory-container">
    <select class="form-select"
            name="Input.Subcategory"
            id="subcategory"
            disabled="@(Model.Options.Count == 0)">

        @if (Model.Options.Count == 0)
        {
            <option value="">Select a category first</option>
        }
        else
        {
            <option value="">Select subcategory...</option>
            @foreach (var option in Model.Options)
            {
                <option value="@option" selected="@(option == Model.Selected)">@option</option>
            }
        }
    </select>
</div>
```

**Key Fix:**
- Uses `disabled="@(Model.Options.Count == 0)"` instead of the old conditional string approach
- Uses `selected="@(option == Model.Selected)"` for proper boolean attribute binding

### 2.4 Add the Subcategory Handler

**File: `Pages/Tasks/Index.cshtml.cs` (add to #region Dependent Dropdowns)**

```csharp
#region Dependent Dropdowns

/// <summary>
/// Returns the subcategory dropdown options based on selected category.
/// Called when category dropdown changes.
/// </summary>
public IActionResult OnGetSubcategories([FromQuery(Name = "Input.Category")] string? category)
{
    var subcategories = CategoryData.GetSubcategories(category);
    return Fragment("Partials/_SubcategorySelect", (subcategories, (string?)null));
}

#endregion
```

**Important:** Uses `[FromQuery(Name = "Input.Category")]` to bind the correct query parameter.

### 2.5 Add Category/Subcategory to the Form

**File: `Pages/Tasks/Partials/_TaskForm.cshtml` (add before Tags section)**

```cshtml
@* Category and Subcategory dropdowns *@
<div class="row">
    <div class="col-md-6 mb-3 mb-md-0">
        <label class="form-label" for="category">Category</label>
        <select class="form-select"
                name="Input.Category"
                id="category"
                asp-for="Input.Category"
                hx-get="?handler=Subcategories"
                hx-target="#subcategory-container"
                hx-swap="outerHTML">
            <option value="">Select category...</option>
            @foreach (var cat in RazorPagesHtmxWorkshop.Data.CategoryData.GetCategories())
            {
                <option value="@cat">@cat</option>
            }
        </select>
    </div>

    <div class="col-md-6">
        <label class="form-label" for="subcategory">Subcategory</label>
        @{
            var currentSubcategories = RazorPagesHtmxWorkshop.Data.CategoryData
                .GetSubcategories(Model.Input.Category);
        }
        <partial name="Partials/_SubcategorySelect"
                 model="(currentSubcategories, Model.Input.Subcategory)" />
    </div>
</div>

@* Dynamic Tags section *@
<partial name="Partials/_TagsContainer" model="Model.Input.Tags" />
```

### 2.6 Test Dependent Dropdowns

1. **Navigate** to `/Tasks`
2. **Select a Category** (e.g., "Work")
3. **Observe**: Subcategory dropdown updates with relevant options
4. **Select a different Category**
5. **Observe**: Subcategory options change accordingly
6. **Clear Category** (select placeholder)
7. **Observe**: Subcategory becomes disabled

---

## Pattern 3: Enhanced Task Details Modal (10 minutes)

Now that we have tags and categories, let's update the details modal to show ALL task information.

### 3.1 Update the TaskDetails Partial

**File: `Pages/Tasks/Partials/_TaskDetails.cshtml`**

```cshtml
@using RazorPagesHtmxWorkshop.Models
@model TaskItem?

@*
    Task Details Modal
    ==================

    Target ID: #task-modal-container
    Purpose: Display detailed information about a single task in a modal
    Model: TaskItem? (null if not found)

    Lab 5: Now includes Category, Subcategory, Tags, Status, and all metadata
*@

<div class="modal fade" id="task-modal" tabindex="-1" aria-labelledby="taskModalLabel" aria-hidden="true">
    <div class="modal-dialog modal-dialog-centered">
        <div class="modal-content workshop-card" style="background-color: #f8f9fa;">
            <div class="modal-header border-bottom border-light">
                <h5 class="modal-title text-dark" id="taskModalLabel">Task Details</h5>
                <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close" style="filter: brightness(0);"></button>
            </div>
            <div class="modal-body">
                @if (Model is null)
                {
                    <div class="text-dark text-center py-4">Task not found.</div>
                }
                else
                {
                    <div class="vstack gap-4 py-2">
                        <div>
                            <label class="form-label text-dark small mb-1">Title</label>
                            <div class="fw-semibold fs-5 text-dark">@Model.Title</div>
                        </div>

                        <div class="row">
                            <div class="col-6">
                                <label class="form-label text-dark small mb-1">Category</label>
                                <div class="fw-medium text-dark">@(Model.Category ?? "None")</div>
                            </div>
                            <div class="col-6">
                                <label class="form-label text-dark small mb-1">Subcategory</label>
                                <div class="fw-medium text-dark">@(Model.Subcategory ?? "None")</div>
                            </div>
                        </div>

                        <div>
                            <label class="form-label text-dark small mb-1">Tags</label>
                            <div>
                                @if (Model.Tags.Any())
                                {
                                    @foreach (var tag in Model.Tags)
                                    {
                                        <span class="badge rounded-pill border border-secondary text-dark me-1">@tag</span>
                                    }
                                }
                                else
                                {
                                    <span class="text-dark italic small">No tags</span>
                                }
                            </div>
                        </div>

                        <div class="row">
                            <div class="col-6">
                                <label class="form-label text-dark small mb-1">Status</label>
                                <div>
                                    @if (Model.IsDone)
                                    {
                                        <span class="badge text-bg-success">Done</span>
                                    }
                                    else
                                    {
                                        <span class="badge text-bg-secondary">Open</span>
                                    }
                                </div>
                            </div>
                            <div class="col-6">
                                <label class="form-label text-dark small mb-1">ID</label>
                                <div class="text-dark">@Model.Id</div>
                            </div>
                        </div>

                        <div>
                            <label class="form-label text-dark small mb-1">Created</label>
                            <div class="text-dark">@Model.CreatedUtc.ToLocalTime().ToString("F")</div>
                        </div>
                    </div>
                }
            </div>
            <div class="modal-footer border-top border-light">
                <button type="button" class="btn btn-outline-secondary" data-bs-dismiss="modal">Close</button>
            </div>
        </div>
    </div>
</div>
```

**New Features:**
- Shows Category and Subcategory
- Displays Tags as badges
- Shows Task Status (Done/Open)
- Displays Task ID and Created timestamp
- Better visual organization with grouped fields

---

## Pattern 4: Long-Running Operations with Polling (15–20 minutes)

Some operations take time—file processing, report generation, external API calls. Instead of making users wait on a loading spinner, you can show progress updates via polling.

### 4.1 Create a Simple Job Simulation

For the workshop, we'll simulate a background job with in-memory state:

**File: `Data/JobSimulator.cs` (create new file)**

```csharp
namespace RazorPagesHtmxWorkshop.Data;

/// <summary>
/// Simulates a long-running background job for demonstration.
/// In production, you'd use a proper job queue (Hangfire, etc.).
/// </summary>
public static class JobSimulator
{
    private static readonly Dictionary<string, JobStatus> _jobs = new();
    private static readonly object _lock = new();

    public class JobStatus
    {
        public string JobId { get; init; } = "";
        public string State { get; set; } = "pending"; // pending, running, completed, failed
        public int Progress { get; set; } = 0;
        public string? Result { get; set; }
        public string? Error { get; set; }
        public DateTime StartedAt { get; init; } = DateTime.UtcNow;
        public DateTime? CompletedAt { get; set; }
    }

    /// <summary>
    /// Starts a new simulated job.
    /// </summary>
    public static JobStatus StartJob()
    {
        var jobId = Guid.NewGuid().ToString("N")[..8];
        var status = new JobStatus
        {
            JobId = jobId,
            State = "running",
            Progress = 0
        };

        lock (_lock)
        {
            _jobs[jobId] = status;
        }

        // Simulate progress in background
        _ = Task.Run(async () =>
        {
            try
            {
                for (var i = 1; i <= 10; i++)
                {
                    await Task.Delay(500); // Simulate work

                    lock (_lock)
                    {
                        if (_jobs.TryGetValue(jobId, out var job))
                        {
                            job.Progress = i * 10;
                        }
                    }
                }

                lock (_lock)
                {
                    if (_jobs.TryGetValue(jobId, out var job))
                    {
                        job.State = "completed";
                        job.Progress = 100;
                        job.Result = $"Report generated successfully at {DateTime.Now:HH:mm:ss}";
                        job.CompletedAt = DateTime.UtcNow;
                    }
                }
            }
            catch (Exception ex)
            {
                lock (_lock)
                {
                    if (_jobs.TryGetValue(jobId, out var job))
                    {
                        job.State = "failed";
                        job.Error = ex.Message;
                        job.CompletedAt = DateTime.UtcNow;
                    }
                }
            }
        });

        return status;
    }

    /// <summary>
    /// Gets the current status of a job.
    /// </summary>
    public static JobStatus? GetStatus(string jobId)
    {
        lock (_lock)
        {
            return _jobs.TryGetValue(jobId, out var status) ? status : null;
        }
    }

    /// <summary>
    /// Cleans up old jobs (call periodically in production).
    /// </summary>
    public static void Cleanup(TimeSpan maxAge)
    {
        var cutoff = DateTime.UtcNow - maxAge;
        lock (_lock)
        {
            var oldJobs = _jobs
                .Where(kv => kv.Value.StartedAt < cutoff)
                .Select(kv => kv.Key)
                .ToList();

            foreach (var id in oldJobs)
            {
                _jobs.Remove(id);
            }
        }
    }
}
```

### 4.2 Create the Job Status Fragment

**File: `Pages/Tasks/Partials/_JobStatus.cshtml`**

```cshtml
@using RazorPagesHtmxWorkshop.Data
@model JobSimulator.JobStatus?

@*
    Job Status Fragment
    ===================

    Purpose: Display job progress and result
    Model: JobStatus (or null if no job)

    Design notes:
    - Fragment contains polling trigger when job is running
    - Polling stops automatically when job completes/fails
    - Uses hx-trigger="every 1s" for active polling
    - Conditional rendering based on job state
*@

<div id="job-status">
    @if (Model is null)
    {
        <div class="card">
            <div class="card-body">
                <h5 class="card-title">Generate Report</h5>
                <p class="card-text text-muted">
                    Click the button to generate a sample report.
                    This simulates a long-running operation.
                </p>
                <button type="button"
                        class="btn btn-primary"
                        hx-post="?handler=StartJob"
                        hx-target="#job-status"
                        hx-swap="outerHTML">
                    Start Report Generation
                </button>
            </div>
        </div>
    }
    else if (Model.State == "running")
    {
        @* Active job - include polling trigger *@
        <div class="card border-primary"
             hx-get="?handler=JobStatus&jobId=@Model.JobId"
             hx-trigger="every 1s"
             hx-target="#job-status"
             hx-swap="outerHTML">
            <div class="card-body">
                <h5 class="card-title">
                    <span class="spinner-border spinner-border-sm me-2" role="status"></span>
                    Generating Report...
                </h5>
                <div class="progress mb-3" style="height: 25px;">
                    <div class="progress-bar progress-bar-striped progress-bar-animated"
                         role="progressbar"
                         style="width: @Model.Progress%"
                         aria-valuenow="@Model.Progress"
                         aria-valuemin="0"
                         aria-valuemax="100">
                        @Model.Progress%
                    </div>
                </div>
                <p class="card-text text-muted small mb-0">
                    Job ID: @Model.JobId | Started: @Model.StartedAt.ToLocalTime().ToString("HH:mm:ss")
                </p>
            </div>
        </div>
    }
    else if (Model.State == "completed")
    {
        @* Completed - no polling *@
        <div class="card border-success">
            <div class="card-body">
                <h5 class="card-title text-success">
                    <span class="me-2">✓</span> Report Complete
                </h5>
                <p class="card-text">@Model.Result</p>
                <p class="card-text text-muted small">
                    Completed in @((Model.CompletedAt!.Value - Model.StartedAt).TotalSeconds.ToString("F1")) seconds
                </p>
                <button type="button"
                        class="btn btn-outline-primary"
                        hx-get="?handler=ResetJob"
                        hx-target="#job-status"
                        hx-swap="outerHTML">
                    Generate Another Report
                </button>
            </div>
        </div>
    }
    else if (Model.State == "failed")
    {
        @* Failed - no polling *@
        <div class="card border-danger">
            <div class="card-body">
                <h5 class="card-title text-danger">
                    <span class="me-2">✗</span> Report Failed
                </h5>
                <p class="card-text text-danger">@Model.Error</p>
                <button type="button"
                        class="btn btn-outline-primary"
                        hx-get="?handler=ResetJob"
                        hx-target="#job-status"
                        hx-swap="outerHTML">
                    Try Again
                </button>
            </div>
        </div>
    }
</div>
```

### 4.3 Create the OOB Job Status Fragment

For job completion, we want to update both the job status AND show a message:

**File: `Pages/Tasks/Partials/_JobStatusWithOob.cshtml`**

```cshtml
@using RazorPagesHtmxWorkshop.Data
@model (JobSimulator.JobStatus Status, string Message, string AlertClass)

@*
    Job Status with OOB Message
    ===========================

    Purpose: Returns job status + out-of-band message in one response
    Model: Tuple with status, message text, and Bootstrap alert class

    Design notes:
    - Primary content swaps into #job-status (via hx-target)
    - OOB content swaps into #messages automatically
    - htmx processes both fragments from single response
*@

@* Primary fragment - swapped into hx-target *@
<div id="job-status">
    @if (Model.Status.State == "completed")
    {
        <div class="card border-success">
            <div class="card-body">
                <h5 class="card-title text-success">
                    <span class="me-2">✓</span> Report Complete
                </h5>
                <p class="card-text">@Model.Status.Result</p>
                <p class="card-text text-muted small">
                    Completed in @((Model.Status.CompletedAt!.Value - Model.Status.StartedAt).TotalSeconds.ToString("F1")) seconds
                </p>
                <button type="button"
                        class="btn btn-outline-primary"
                        hx-get="?handler=ResetJob"
                        hx-target="#job-status"
                        hx-swap="outerHTML">
                    Generate Another Report
                </button>
            </div>
        </div>
    }
    else
    {
        <div class="card border-danger">
            <div class="card-body">
                <h5 class="card-title text-danger">
                    <span class="me-2">✗</span> Report Failed
                </h5>
                <p class="card-text text-danger">@Model.Status.Error</p>
                <button type="button"
                        class="btn btn-outline-primary"
                        hx-get="?handler=ResetJob"
                        hx-target="#job-status"
                        hx-swap="outerHTML">
                    Try Again
                </button>
            </div>
        </div>
    }
</div>

@* Out-of-band fragment - swaps into #messages automatically *@
<div id="messages" hx-swap-oob="true">
    <div class="alert alert-@Model.AlertClass alert-dismissible fade show" role="alert">
        @Model.Message
        <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
    </div>
</div>
```

### 4.4 Add the Job Handlers

**File: `Pages/Tasks/Index.cshtml.cs` (add to #region Long-Running Jobs)**

```csharp
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
```

### 4.5 Add Job Status to the Page

**File: `Pages/Tasks/Index.cshtml` (add section for job demo)**

```cshtml
@* Long-Running Job Demo *@
<div class="row mt-4">
    <div class="col-12">
        <h2 class="h5">Background Job Demo</h2>
        <partial name="Partials/_JobStatus" model="@((JobSimulator.JobStatus?)null)" />
    </div>
</div>
```

Add the using statement at the top:

```cshtml
@using RazorPagesHtmxWorkshop.Data
```

---

## Complete Handler Inventory (Lab 5)

| Handler              | Verb | Returns                             | Purpose                         |
|----------------------|------|-------------------------------------|---------------------------------|
| `OnGetAddTag`        | GET  | `_TagRow`                           | Add new tag input               |
| `OnGetRemoveTag`     | GET  | Empty                               | (Not used - removal client-side)|
| `OnGetSubcategories` | GET  | `_SubcategorySelect`                | Update subcategory dropdown     |
| `OnPostStartJob`     | POST | `_JobStatus`                        | Start background job            |
| `OnGetJobStatus`     | GET  | `_JobStatus` or `_JobStatusWithOob` | Poll job progress               |
| `OnGetResetJob`      | GET  | `_JobStatus` (null)                 | Reset job UI                    |

---

## Verification Checklist

Before completing the workshop, verify these behaviors:

### Dynamic Tags

- [ ] "Add Tag" button appends new tag input
- [ ] Each tag has a working remove button (client-side, instant)
- [ ] Multiple tags can be added
- [ ] Tags are included when form submits
- [ ] Form reset clears all tags
- [ ] Flash message shows tag count on success

### Dependent Dropdowns

- [ ] Selecting a category updates subcategory options
- [ ] Changing category updates subcategory again
- [ ] Clearing category disables subcategory
- [ ] Selected values persist on validation failure
- [ ] Category and subcategory are saved with task

### Enhanced Task Details

- [ ] Details modal shows Category and Subcategory
- [ ] Details modal shows all Tags as badges
- [ ] Details modal shows Task Status (Done/Open)
- [ ] Details modal shows Task ID and Created timestamp

### Polling

- [ ] Starting job shows progress card
- [ ] Progress updates every second
- [ ] Network tab shows polling requests
- [ ] Polling stops when job completes
- [ ] Success/failure state displays correctly

### OOB Swaps

- [ ] Job completion updates both job card and messages
- [ ] Single network response contains both fragments
- [ ] Messages area shows appropriate alert

---

## Key Fixes and Improvements in Lab 5

### 1. Simplified Tag Implementation

**Before (documented):**
- Required server-side removal handler
- Complex index tracking with `hx-vals`
- Individual IDs for each tag row

**After (actual code):**
- Client-side removal with `hx-on:click`
- Simple `name="Input.Tags"` binding
- No index management needed

### 2. Fixed Category/Subcategory Attributes

**Before (old syntax):**
```cshtml
@(Model.Options.Count == 0 ? "disabled" : "")
```

**After (proper Razor):**
```cshtml
disabled="@(Model.Options.Count == 0)"
```

### 3. Enhanced Task Details Modal

Now shows complete task information:
- Category and Subcategory
- Tags (with badge styling)
- Status (Done/Open badge)
- Task ID
- Created timestamp

### 4. Proper Parameter Binding

Uses `[FromQuery(Name = "Input.Category")]` for subcategory handler to correctly bind the parameter from the htmx request.

---

## Key Takeaways

### Pattern Summary

| Pattern                 | Key Technique                                | When to Use             |
|-------------------------|----------------------------------------------|-------------------------|
| **Add/Remove Rows**     | `hx-swap="beforeend"` + client-side removal  | Dynamic sub-collections |
| **Dependent Dropdowns** | `hx-get` on change + proper parameter binding| Cascading selections    |
| **Polling**             | `hx-trigger="every Xs"` in fragment          | Long-running operations |
| **OOB Swaps**           | `hx-swap-oob="true"` on additional fragments | Multi-region updates    |

### The Server Controls Everything

In all four patterns, the server decides:

- **What HTML to render** (the fragments)
- **Whether to continue polling** (by including/excluding trigger)
- **What else to update** (via OOB fragments)
- **Validation and business logic** (tags cleanup, category relationships)

This is the power of hypermedia: the server remains in control of application state.

---

## Troubleshooting

### Common Issues

| Problem                    | Likely Cause                    | Solution                           |
|----------------------------|---------------------------------|------------------------------------|
| Tags not binding           | Wrong name attribute            | Use `name="Input.Tags"` (no index) |
| Remove button doesn't work | Missing hx-on:click             | Add `hx-on:click` attribute        |
| Dropdown doesn't update    | Missing parameter binding       | Use `[FromQuery]` attribute        |
| Polling doesn't stop       | Trigger in completed state      | Remove trigger when done           |
| OOB swap fails             | Target ID doesn't exist         | Ensure target element exists       |
| Details missing info       | Old partial version             | Update `_TaskDetails.cshtml`       |

---

## Workshop Wrap-Up

Congratulations! You've completed Lab 5 and mastered:

- **Dynamic forms** with add/remove functionality
- **Dependent dropdowns** with cascading selection
- **Long-running operations** with polling
- **Out-of-band swaps** for multi-region updates
- **Enhanced UI patterns** showing complete data models

**Next Steps:**
1. Apply these patterns to your real projects
2. Explore htmx extensions (SSE, WebSockets)
3. Read the htmx documentation at htmx.org
4. Join the htmx community

**Thank you for completing the workshop!**
