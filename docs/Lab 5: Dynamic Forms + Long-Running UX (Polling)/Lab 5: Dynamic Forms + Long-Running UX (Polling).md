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

| Scenario | Traditional Approach | htmx Approach |
|----------|---------------------|---------------|
| Add form row | JavaScript clones DOM, manages indexes | Server renders new row fragment |
| Dependent dropdown | JavaScript fetches JSON, builds options | Server renders options fragment |
| Long-running job | WebSocket or complex polling library | `hx-trigger="every 1s"` + status endpoint |
| Multiple updates | Custom event system | `hx-swap-oob` for out-of-band swaps |

---

## Lab Outcomes

By the end of Lab 5, you will be able to:

| Outcome | Description |
|---------|-------------|
| **Add/Remove rows** | Dynamic sub-collections (tags, line items) via fragment endpoints |
| **Dependent dropdowns** | Cascading selects (Category → Subcategory) |
| **Polling** | Long-running operations with `hx-trigger="every Xs"` |
| **Out-of-band swaps** | Update multiple page regions from a single response |
| **Conditional polling** | Start/stop polling based on job status |

---

## Prerequisites

Before starting this lab, ensure you have:

- **Completed Lab 4** with all verifications passing
- **Production-livable conventions** in place (checkpoint complete)
- **Working CRUD operations** for Tasks
- **Fragment helpers** (`IsHtmx()` and `Fragment()`) ready

---

## Pattern 1: Dynamic Form Rows (Add/Remove) (15–20 minutes)

This pattern lets users add and remove items in a sub-collection—like tags on a task, line items on an order, or attendees on an event.

### 1.1 The Design

**User Flow:**

1. User sees a task form with a "Tags" section
2. User clicks "Add Tag" → new empty tag input appears
3. User can remove any tag by clicking its "Remove" button
4. On form submit, all tags are collected and saved

**Architecture:**

- Tags are rendered as a list of inputs inside a `#tags-container`
- "Add Tag" button fetches a new tag row fragment
- Each tag row has a "Remove" button that removes itself
- Server maintains tag indexes for model binding

### 1.2 Update the Input Model

First, extend `NewTaskInput` to support tags:

**File: `Pages/Tasks/Index.cshtml.cs` (update NewTaskInput)**

```csharp
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
}
```

### 1.3 Create the Tag Row Fragment

This fragment renders a single tag input with its remove button:

**File: `Pages/Tasks/Partials/_TagRow.cshtml`**

```cshtml
@model (int Index, string Value)

@*
    Tag Row Fragment
    ================

    Purpose: Single tag input with remove button
    Model: (int Index, string Value) - tuple with index and current value

    Design notes:
    - Index is used for model binding (Input.Tags[0], Input.Tags[1], etc.)
    - Remove button removes this row from DOM (no server round-trip needed)
    - Each row has unique ID based on index for potential targeting

    Swap strategy:
    - Added via hx-swap="beforeend" into #tags-container
    - Removed via hyperscript or hx-swap="delete"
*@

<div class="tag-row input-group mb-2" id="tag-row-@Model.Index">
    <input type="text"
           class="form-control"
           name="Input.Tags[@Model.Index]"
           value="@Model.Value"
           placeholder="Enter tag..." />

    <button type="button"
            class="btn btn-outline-danger"
            hx-get="?handler=RemoveTag&index=@Model.Index"
            hx-target="#tag-row-@Model.Index"
            hx-swap="delete"
            title="Remove tag">
        <span aria-hidden="true">&times;</span>
        <span class="visually-hidden">Remove tag</span>
    </button>
</div>
```

### 1.4 Understanding the Remove Pattern

```html
<button hx-get="?handler=RemoveTag&index=@Model.Index"
        hx-target="#tag-row-@Model.Index"
        hx-swap="delete">
```

| Attribute | Value | Purpose |
|-----------|-------|---------|
| `hx-get` | `"?handler=RemoveTag&index=..."` | Request to remove handler |
| `hx-target` | `"#tag-row-@Model.Index"` | Target this specific row |
| `hx-swap` | `"delete"` | Remove the target from DOM |

**Why `hx-swap="delete"`?**

The `delete` swap strategy removes the target element entirely. The server doesn't need to return anything—an empty 200 response is sufficient.

**Alternative: Client-side removal with hyperscript**

If you want to avoid a server round-trip for simple removal:

```html
<button type="button"
        class="btn btn-outline-danger"
        _="on click remove closest .tag-row">
    &times;
</button>
```

This uses hyperscript (htmx's companion library) for purely client-side DOM manipulation.

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
    - Server tracks next available index via hx-vals
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
            hx-vals='{"nextIndex": @Model.Count}'
            hx-target="#tags-list"
            hx-swap="beforeend">
        <span aria-hidden="true">+</span> Add Tag
    </button>

    <div class="form-text">Add tags to categorize your task.</div>
</div>
```

### 1.6 Understanding the Add Pattern

```html
<button hx-get="?handler=AddTag"
        hx-vals='{"nextIndex": @Model.Count}'
        hx-target="#tags-list"
        hx-swap="beforeend">
```

| Attribute | Value | Purpose |
|-----------|-------|---------|
| `hx-get` | `"?handler=AddTag"` | Request new tag row |
| `hx-vals` | `'{"nextIndex": @Model.Count}'` | Pass the next available index |
| `hx-target` | `"#tags-list"` | Append to the tags list |
| `hx-swap` | `"beforeend"` | Insert as last child |

**Why pass `nextIndex`?**

Model binding in ASP.NET Core uses indexed names like `Input.Tags[0]`, `Input.Tags[1]`. The server needs to know which index to use for the new row.

**The Index Problem:**

If you have tags at indexes 0, 1, 2 and remove index 1, you have a gap. This is actually fine for model binding—it handles sparse arrays. But for simplicity, we track `nextIndex` as the count of current tags.

### 1.7 Add the Handlers

**File: `Pages/Tasks/Index.cshtml.cs` (add these handlers)**

```csharp
/// <summary>
/// Returns a new tag row fragment.
/// The nextIndex parameter ensures unique name attributes for model binding.
/// </summary>
public IActionResult OnGetAddTag(int nextIndex)
{
    // Return a new empty tag row with the next available index
    return Fragment("Partials/_TagRow", (nextIndex, ""));
}

/// <summary>
/// Handles tag removal. Returns empty response for hx-swap="delete".
/// </summary>
public IActionResult OnGetRemoveTag(int index)
{
    // Nothing to return - the delete swap will remove the element
    // We could track removed indexes if needed for server-side state
    return new EmptyResult();
}
```

### 1.8 Update the Form to Include Tags

**File: `Pages/Tasks/Partials/_TaskForm.cshtml` (add tags section)**

```cshtml
@model RazorHtmxWorkshop.Pages.Tasks.IndexModel

<div id="task-form">
    <form method="post" asp-page-handler="Create"
          hx-post="?handler=Create"
          hx-target="#task-list"
          hx-swap="outerHTML"
          hx-indicator="#task-loading">

        @Html.AntiForgeryToken()

        <div asp-validation-summary="ModelOnly" class="text-danger mb-3"></div>

        @* Title field with real-time validation *@
        <div class="mb-3">
            <label class="form-label" for="title">Task Title</label>
            <input id="title"
                   class="form-control"
                   asp-for="Input.Title"
                   placeholder="e.g., Add htmx to Razor Pages"
                   autocomplete="off"
                   hx-post="?handler=ValidateTitle"
                   hx-trigger="keyup changed delay:500ms"
                   hx-target="#title-validation"
                   hx-swap="outerHTML"
                   hx-include="closest form" />
            <span class="text-danger" asp-validation-for="Input.Title"></span>
            <partial name="Partials/_TitleValidation" model="@((string?)null)" />
        </div>

        @* Dynamic Tags section *@
        <partial name="Partials/_TagsContainer" model="Model.Input.Tags" />

        <button class="btn btn-primary" type="submit">Add Task</button>
    </form>
</div>
```

### 1.9 Update OnPostCreate to Handle Tags

**File: `Pages/Tasks/Index.cshtml.cs` (update OnPostCreate)**

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
            Response.StatusCode = 422;
            Response.Headers["HX-Retarget"] = "#task-form";
            Response.Headers["HX-Reswap"] = "outerHTML";
            return Fragment("Partials/_TaskForm", this);
        }

        return Page();
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

    Tasks = InMemoryTaskStore.All();

    if (IsHtmx())
    {
        // Reset input including tags for the form refresh
        Input = new NewTaskInput();

        FlashMessage = $"Task added with {Input.Tags.Count} tag(s).";
        Response.Headers["HX-Trigger"] = "showMessage,clearForm";

        var vm = new TaskListVm
        {
            Items = Tasks.Take(5).ToList(),
            Page = 1,
            PageSize = 5,
            Total = Tasks.Count,
            Query = null
        };

        return Fragment("Partials/_TaskList", vm);
    }

    FlashMessage = "Task added.";
    return RedirectToPage();
}
```

### 1.10 Test Dynamic Tags

1. **Navigate** to `/Tasks`
2. **Click "Add Tag"** → New tag input appears
3. **Add multiple tags** by clicking "Add Tag" repeatedly
4. **Remove a tag** by clicking the × button
5. **Submit the form** with tags
6. **Verify** in console or debugger that tags are received

---

## Pattern 2: Dependent Dropdowns (10–15 minutes)

Dependent dropdowns (cascading selects) update one dropdown based on another's selection—like Category → Subcategory, Country → City, or Make → Model.

### 2.1 The Design

**User Flow:**

1. User selects a Category from the first dropdown
2. Subcategory dropdown updates with relevant options
3. User selects a Subcategory
4. Form can be submitted with both values

**Architecture:**

- Category dropdown has `hx-get` that fetches subcategory options
- Subcategory dropdown is wrapped in a swappable container
- Server returns the subcategory `<select>` fragment

### 2.2 Create Sample Data

For this example, we'll use simple in-memory data:

**File: `Data/CategoryData.cs` (create new file)**

```csharp
namespace RazorHtmxWorkshop.Data;

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

### 2.3 Update the Input Model

**File: `Pages/Tasks/Index.cshtml.cs` (update NewTaskInput)**

```csharp
public class NewTaskInput
{
    [Required(ErrorMessage = "Title is required.")]
    [StringLength(60, MinimumLength = 3, ErrorMessage = "Title must be 3–60 characters.")]
    public string Title { get; set; } = "";

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

### 2.4 Create the Subcategory Fragment

**File: `Pages/Tasks/Partials/_SubcategorySelect.cshtml`**

```cshtml
@model (IReadOnlyList<string> Options, string? Selected)

@*
    Subcategory Select Fragment
    ===========================

    Purpose: Dropdown for subcategory selection
    Model: (Options list, Selected value)

    Design notes:
    - Wrapper div has stable ID for swapping
    - Select is disabled when no options available
    - Options include a placeholder prompt
*@

<div id="subcategory-container">
    <select class="form-select"
            name="Input.Subcategory"
            id="subcategory"
            @(Model.Options.Count == 0 ? "disabled" : "")>

        @if (Model.Options.Count == 0)
        {
            <option value="">Select a category first</option>
        }
        else
        {
            <option value="">Select subcategory...</option>
            @foreach (var option in Model.Options)
            {
                var selected = option == Model.Selected ? "selected" : "";
                <option value="@option" @selected>@option</option>
            }
        }
    </select>
</div>
```

### 2.5 Add the Subcategory Handler

**File: `Pages/Tasks/Index.cshtml.cs` (add handler)**

```csharp
/// <summary>
/// Returns the subcategory dropdown options based on selected category.
/// Called when category dropdown changes.
/// </summary>
public IActionResult OnGetSubcategories(string? category)
{
    var subcategories = CategoryData.GetSubcategories(category);
    return Fragment("Partials/_SubcategorySelect", (subcategories, (string?)null));
}
```

### 2.6 Add the Category Section to the Form

**File: `Pages/Tasks/Partials/_TaskForm.cshtml` (add before Tags section)**

```cshtml
@* Category and Subcategory dropdowns *@
<div class="row mb-3">
    <div class="col-md-6">
        <label class="form-label" for="category">Category</label>
        <select class="form-select"
                name="Input.Category"
                id="category"
                asp-for="Input.Category"
                hx-get="?handler=Subcategories"
                hx-target="#subcategory-container"
                hx-swap="outerHTML"
                hx-include="[name='Input.Category']">
            <option value="">Select category...</option>
            @foreach (var cat in RazorHtmxWorkshop.Data.CategoryData.GetCategories())
            {
                <option value="@cat">@cat</option>
            }
        </select>
    </div>

    <div class="col-md-6">
        <label class="form-label" for="subcategory">Subcategory</label>
        @{
            var currentSubcategories = RazorHtmxWorkshop.Data.CategoryData
                .GetSubcategories(Model.Input.Category);
        }
        <partial name="Partials/_SubcategorySelect"
                 model="(currentSubcategories, Model.Input.Subcategory)" />
    </div>
</div>
```

### 2.7 Understanding the Dependent Dropdown Pattern

```html
<select hx-get="?handler=Subcategories"
        hx-target="#subcategory-container"
        hx-swap="outerHTML"
        hx-include="[name='Input.Category']">
```

| Attribute | Value | Purpose |
|-----------|-------|---------|
| `hx-get` | `"?handler=Subcategories"` | Fetch new options |
| `hx-target` | `"#subcategory-container"` | Replace subcategory dropdown |
| `hx-swap` | `"outerHTML"` | Replace entire container |
| `hx-include` | `"[name='Input.Category']"` | Include this select's value |

**Default Trigger:**

For `<select>` elements, htmx defaults to `hx-trigger="change"`, so we don't need to specify it.

**Why `hx-include`?**

By default, htmx only sends the triggering element's value for `hx-get`. We use `hx-include` to ensure the category value is sent as a query parameter.

### 2.8 Test Dependent Dropdowns

1. **Navigate** to `/Tasks`
2. **Select a Category** (e.g., "Work")
3. **Observe**: Subcategory dropdown updates with relevant options
4. **Select a different Category**
5. **Observe**: Subcategory options change accordingly
6. **Clear Category** (select placeholder)
7. **Observe**: Subcategory becomes disabled

---

## Pattern 3: Long-Running Operations with Polling (15–20 minutes)

Some operations take time—file processing, report generation, external API calls. Instead of making users wait on a loading spinner, you can show progress updates via polling.

### 3.1 The Design

**User Flow:**

1. User clicks "Generate Report" (or similar action)
2. Server starts the job and returns a "job started" response
3. Page begins polling a status endpoint every second
4. Status fragment updates with progress (10%, 50%, 90%...)
5. When complete, polling stops and final result is shown

**Architecture:**

- Job submission handler starts the job and returns initial status
- Status endpoint returns current progress
- Polling uses `hx-trigger="every 1s"` with conditional stop
- `hx-swap-oob` can update other regions simultaneously

### 3.2 Create a Simple Job Simulation

For the workshop, we'll simulate a background job with in-memory state:

**File: `Data/JobSimulator.cs` (create new file)**

```csharp
namespace RazorHtmxWorkshop.Data;

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

### 3.3 Create the Job Status Fragment

**File: `Pages/Tasks/Partials/_JobStatus.cshtml`**

```cshtml
@using RazorHtmxWorkshop.Data
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

### 3.4 Understanding Conditional Polling

The key to this pattern is that **polling is defined in the fragment itself**:

```html
<!-- Only present when job is running -->
<div hx-get="?handler=JobStatus&jobId=@Model.JobId"
     hx-trigger="every 1s"
     hx-target="#job-status"
     hx-swap="outerHTML">
```

| Attribute | Value | Purpose |
|-----------|-------|---------|
| `hx-get` | `"?handler=JobStatus&jobId=..."` | Poll status endpoint |
| `hx-trigger` | `"every 1s"` | Fire every 1 second |
| `hx-target` | `"#job-status"` | Replace this fragment |
| `hx-swap` | `"outerHTML"` | Replace entire element |

**How Polling Stops:**

When the job completes, the server returns a fragment **without** the `hx-trigger="every 1s"` attribute. Since htmx attributes are on the new content, polling stops automatically.

This is the elegant part: **the server controls whether polling continues by what it renders**.

### 3.5 Add the Job Handlers

**File: `Pages/Tasks/Index.cshtml.cs` (add these handlers)**

```csharp
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
/// </summary>
public IActionResult OnGetJobStatus(string jobId)
{
    var status = JobSimulator.GetStatus(jobId);
    return Fragment("Partials/_JobStatus", status);
}

/// <summary>
/// Resets the job UI to initial state.
/// </summary>
public IActionResult OnGetResetJob()
{
    return Fragment("Partials/_JobStatus", null);
}
```

### 3.6 Add Job Status to the Page

**File: `Pages/Tasks/Index.cshtml` (add a section for the job demo)**

```cshtml
@* Long-Running Job Demo *@
<div class="row mt-4">
    <div class="col-12">
        <h2 class="h5">Background Job Demo</h2>
        <partial name="Partials/_JobStatus" model="@((JobSimulator.JobStatus?)null)" />
    </div>
</div>
```

Add the using statement at the top of the page:

```cshtml
@using RazorHtmxWorkshop.Data
```

### 3.7 Test Polling

1. **Navigate** to `/Tasks`
2. **Click "Start Report Generation"**
3. **Observe**: Progress bar updates every second
4. **Watch Network tab**: See requests firing every ~1 second
5. **Wait for completion** (about 5 seconds)
6. **Observe**: Polling stops, success message shows
7. **Click "Generate Another Report"** to repeat

---

## Pattern 4: Out-of-Band Swaps (10–15 minutes)

Sometimes a single action needs to update multiple page regions. Out-of-band (OOB) swaps let the server include additional fragments that swap into their respective targets automatically.

### 4.1 The Design

**Scenario:**

When a job completes, we want to:

1. Update the job status card (primary swap)
2. Update the messages area with a notification (out-of-band)
3. Maybe update a "recent activity" section (another OOB)

**How It Works:**

- Response includes the primary fragment (swapped normally)
- Response also includes additional fragments with `hx-swap-oob="true"`
- htmx finds each OOB fragment's ID and swaps it into the matching element

### 4.2 Understanding hx-swap-oob

```html
<!-- Primary response (swapped into hx-target) -->
<div id="job-status">
    ... job completed content ...
</div>

<!-- Out-of-band swap (swapped into #messages automatically) -->
<div id="messages" hx-swap-oob="true">
    <div class="alert alert-success">Report generation complete!</div>
</div>
```

The `hx-swap-oob="true"` attribute tells htmx:

1. Find an element with `id="messages"` in the current page
2. Swap this fragment into that element
3. Use `outerHTML` swap by default (can be customized)

### 4.3 Create an OOB-Aware Job Status Response

Let's update the job handlers to use OOB swaps for notifications:

**File: `Pages/Tasks/Index.cshtml.cs` (update OnGetJobStatus)**

```csharp
/// <summary>
/// Returns the current status of a job.
/// When job completes, includes OOB swap for messages.
/// </summary>
public IActionResult OnGetJobStatus(string jobId)
{
    var status = JobSimulator.GetStatus(jobId);

    // For completed or failed jobs, include OOB message
    if (status?.State is "completed" or "failed")
    {
        return JobStatusWithOobMessage(status);
    }

    return Fragment("Partials/_JobStatus", status);
}

/// <summary>
/// Returns job status fragment with an out-of-band message fragment.
/// </summary>
private IActionResult JobStatusWithOobMessage(JobSimulator.JobStatus status)
{
    // Build the combined response with OOB content
    var message = status.State == "completed"
        ? "Report generation completed successfully!"
        : $"Report generation failed: {status.Error}";

    var alertClass = status.State == "completed" ? "success" : "danger";

    // Return a view that combines both fragments
    return new ContentResult
    {
        ContentType = "text/html",
        Content = $@"
            <div id=""job-status"">
                {RenderJobStatusContent(status)}
            </div>
            <div id=""messages"" hx-swap-oob=""true"">
                <div class=""alert alert-{alertClass} alert-dismissible fade show"" role=""alert"">
                    {message}
                    <button type=""button"" class=""btn-close"" data-bs-dismiss=""alert"" aria-label=""Close""></button>
                </div>
            </div>"
    };
}

private string RenderJobStatusContent(JobSimulator.JobStatus status)
{
    if (status.State == "completed")
    {
        return $@"
            <div class=""card border-success"">
                <div class=""card-body"">
                    <h5 class=""card-title text-success"">
                        <span class=""me-2"">✓</span> Report Complete
                    </h5>
                    <p class=""card-text"">{status.Result}</p>
                    <p class=""card-text text-muted small"">
                        Completed in {(status.CompletedAt!.Value - status.StartedAt).TotalSeconds:F1} seconds
                    </p>
                    <button type=""button""
                            class=""btn btn-outline-primary""
                            hx-get=""?handler=ResetJob""
                            hx-target=""#job-status""
                            hx-swap=""outerHTML"">
                        Generate Another Report
                    </button>
                </div>
            </div>";
    }
    else
    {
        return $@"
            <div class=""card border-danger"">
                <div class=""card-body"">
                    <h5 class=""card-title text-danger"">
                        <span class=""me-2"">✗</span> Report Failed
                    </h5>
                    <p class=""card-text text-danger"">{status.Error}</p>
                    <button type=""button""
                            class=""btn btn-outline-primary""
                            hx-get=""?handler=ResetJob""
                            hx-target=""#job-status""
                            hx-swap=""outerHTML"">
                        Try Again
                    </button>
                </div>
            </div>";
    }
}
```

### 4.4 A Cleaner Approach: OOB Partial

Instead of building HTML strings, create a dedicated partial for OOB responses:

**File: `Pages/Tasks/Partials/_JobStatusWithOob.cshtml`**

```cshtml
@using RazorHtmxWorkshop.Data
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

### 4.5 Update Handler to Use OOB Partial

**File: `Pages/Tasks/Index.cshtml.cs` (simplified handler)**

```csharp
public IActionResult OnGetJobStatus(string jobId)
{
    var status = JobSimulator.GetStatus(jobId);

    if (status is null)
    {
        return Fragment("Partials/_JobStatus", null);
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
```

### 4.6 Understanding OOB Swap Strategies

You can customize how OOB content is swapped:

| Syntax | Behavior |
|--------|----------|
| `hx-swap-oob="true"` | Default: `outerHTML` swap |
| `hx-swap-oob="innerHTML"` | Replace target's children |
| `hx-swap-oob="beforeend"` | Append to target |
| `hx-swap-oob="afterbegin"` | Prepend to target |
| `hx-swap-oob="outerHTML:#custom-id"` | Swap into different ID |

**Example: Append to Activity Log**

```html
<div id="activity-log" hx-swap-oob="beforeend">
    <div class="activity-item">New report generated at 3:45 PM</div>
</div>
```

### 4.7 Test OOB Swaps

1. **Navigate** to `/Tasks`
2. **Start a job** and wait for completion
3. **Observe**: Both the job card AND the messages area update
4. **Check Network tab**: Single response contains both fragments

---

## Complete Code Reference

### Index.cshtml.cs (Lab 5 Additions)

Here are the key additions to your PageModel for Lab 5:

```csharp
// Add to using statements
using RazorHtmxWorkshop.Data;

// Add these handlers to IndexModel class:

#region Dynamic Tags

public IActionResult OnGetAddTag(int nextIndex)
{
    return Fragment("Partials/_TagRow", (nextIndex, ""));
}

public IActionResult OnGetRemoveTag(int index)
{
    return new EmptyResult();
}

#endregion

#region Dependent Dropdowns

public IActionResult OnGetSubcategories(string? category)
{
    var subcategories = CategoryData.GetSubcategories(category);
    return Fragment("Partials/_SubcategorySelect", (subcategories, (string?)null));
}

#endregion

#region Long-Running Jobs

public IActionResult OnPostStartJob()
{
    var job = JobSimulator.StartJob();
    return Fragment("Partials/_JobStatus", job);
}

public IActionResult OnGetJobStatus(string jobId)
{
    var status = JobSimulator.GetStatus(jobId);

    if (status is null)
    {
        return Fragment("Partials/_JobStatus", null);
    }

    if (status.State is "completed" or "failed")
    {
        var message = status.State == "completed"
            ? "Report generation completed successfully!"
            : $"Report generation failed: {status.Error}";

        var alertClass = status.State == "completed" ? "success" : "danger";

        return Fragment("Partials/_JobStatusWithOob", (status, message, alertClass));
    }

    return Fragment("Partials/_JobStatus", status);
}

public IActionResult OnGetResetJob()
{
    return Fragment("Partials/_JobStatus", null);
}

#endregion
```

### Handler Inventory (Lab 5)

| Handler | Verb | Returns | Purpose |
|---------|------|---------|---------|
| `OnGetAddTag` | GET | `_TagRow` | Add new tag input |
| `OnGetRemoveTag` | GET | Empty | Remove tag (delete swap) |
| `OnGetSubcategories` | GET | `_SubcategorySelect` | Update subcategory dropdown |
| `OnPostStartJob` | POST | `_JobStatus` | Start background job |
| `OnGetJobStatus` | GET | `_JobStatus` or `_JobStatusWithOob` | Poll job progress |
| `OnGetResetJob` | GET | `_JobStatus` (null) | Reset job UI |

---

## Verification Checklist

Before completing the workshop, verify these behaviors:

### Dynamic Tags

- [ ] "Add Tag" button appends new tag input
- [ ] Each tag has a working remove button
- [ ] Multiple tags can be added
- [ ] Tags are included when form submits
- [ ] Form reset clears all tags

### Dependent Dropdowns

- [ ] Selecting a category updates subcategory options
- [ ] Changing category updates subcategory again
- [ ] Clearing category disables subcategory
- [ ] Selected values persist on validation failure

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

## Key Takeaways

### Pattern Summary

| Pattern | Key Technique | When to Use |
|---------|---------------|-------------|
| **Add/Remove Rows** | `hx-swap="beforeend"` + `hx-swap="delete"` | Dynamic sub-collections |
| **Dependent Dropdowns** | `hx-get` on change + `hx-include` | Cascading selections |
| **Polling** | `hx-trigger="every Xs"` in fragment | Long-running operations |
| **OOB Swaps** | `hx-swap-oob="true"` on additional fragments | Multi-region updates |

### The Server Controls Everything

In all four patterns, the server decides:

- **What HTML to render** (the fragments)
- **Whether to continue polling** (by including/excluding trigger)
- **What else to update** (via OOB fragments)
- **What indexes to use** (for model binding)

This is the power of hypermedia: the server remains in control of application state.

### Polling Best Practices

| Practice | Reason |
|----------|--------|
| Use 1-2 second intervals | Balances responsiveness with server load |
| Stop polling on completion | Prevent unnecessary requests |
| Include job ID in requests | Enable multiple concurrent jobs |
| Show progress feedback | Keep users informed |
| Handle failures gracefully | Display errors, offer retry |

### OOB Swap Guidelines

| Guideline | Reason |
|-----------|--------|
| Use sparingly | Too many OOB swaps become hard to track |
| Document OOB targets | Others need to know what updates |
| Keep OOB content small | Large OOB swaps can be jarring |
| Consider events instead | `HX-Trigger` + listeners may be cleaner |

---

## Troubleshooting

### Common Issues

| Problem | Likely Cause | Solution |
|---------|--------------|----------|
| Tags not binding | Index mismatch | Ensure indexes are sequential |
| Remove button doesn't work | Missing hx-target | Add target to specific row |
| Dropdown doesn't update | Missing hx-include | Include the select's value |
| Polling doesn't stop | Trigger in completed state | Remove trigger when done |
| OOB swap fails | Target ID doesn't exist | Ensure target element exists |
| OOB replaces wrong element | Duplicate IDs | Ensure unique IDs |

### Debug Tips

1. **Network tab**: Verify polling requests and responses
2. **Response body**: Check for OOB fragments in response
3. **Elements panel**: Confirm target elements exist with correct IDs
4. **Console**: Look for htmx processing messages
5. **Form data**: Verify model binding with debugger

---

## Workshop Wrap-Up

Congratulations! You've completed all five labs and two checkpoints. You now have:

### Skills Acquired

| Lab | Skills |
|-----|--------|
| **Lab 1** | Fragment boundaries, partial views, stable IDs |
| **Lab 2** | `hx-get`, `hx-post`, `hx-target`, `hx-swap`, retargeting |
| **Lab 3** | Real-time validation, debouncing, antiforgery |
| **Lab 4** | Modals, confirm dialogs, URL state, pagination |
| **Lab 5** | Dynamic forms, dependent selects, polling, OOB swaps |

### Patterns to Apply

Take these patterns to your real projects:

1. **Fragment-first architecture**: Design UI as swappable regions
2. **Server-driven state**: Let the server control what renders
3. **Progressive enhancement**: Start with working forms, add htmx
4. **Polling for async**: Replace WebSockets with simple polling
5. **OOB for multi-updates**: Update related regions in one response

### Next Steps

1. **Apply to a real project**: Start with one feature
2. **Explore extensions**: SSE, WebSockets, hyperscript
3. **Read the docs**: htmx.org has excellent documentation
4. **Join the community**: htmx Discord, GitHub discussions

**Thank you for attending!**
