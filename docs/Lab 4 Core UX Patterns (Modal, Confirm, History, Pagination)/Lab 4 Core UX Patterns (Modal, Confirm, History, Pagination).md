---
order: 1
icon: code-square
---

# Lab 4: Core UX Patterns (Modal, Confirm, History, Pagination)

## Overview

In this lab, you'll implement the core UX patterns that make htmx-powered applications feel polished and professional. These patterns are the building blocks of real-world interactive applications:

- **Details View**: Load content into a Bootstrap modal without page reload
- **Delete with Confirmation**: Safe destructive actions with user confirmation
- **Filtering and Pagination**: URL-driven state that supports browser back/forward
- **Smooth Transitions**: Visual polish with CSS transitions

By the end of this lab, your Tasks application will feel like a modern single-page application—but with the simplicity of server-rendered HTML.

### The Key Insight

These patterns share a common theme: **the server remains in control**. Instead of building client-side state machines, you let htmx handle the transport while the server decides what HTML to return.

| Traditional SPA      | htmx Approach                       |
|----------------------|-------------------------------------|
| Client manages state | Server renders fragments            |
| Router handles URLs  | `hx-push-url` syncs URL             |
| Modal logic in JS    | Modal body loaded via `hx-get`      |
| Pagination component | Links with `hx-get` + `hx-push-url` |

---

## Lab Outcomes

By the end of Lab 4, you will be able to:

| Outcome                 | Description                                      |
|-------------------------|--------------------------------------------------|
| **Details pattern**     | Load task details into a Bootstrap modal         |
| **Delete with confirm** | Use `hx-confirm` for safe destructive actions    |
| **Filtering**           | Filter the task list with URL state preserved    |
| **Pagination**          | Navigate pages with back/forward support         |
| **URL state**           | Use `hx-push-url` to make URLs bookmarkable      |
| **Swap strategies**     | Understand when to use different swap approaches |
| **Transitions**         | Add CSS transitions for smooth visual feedback   |

---

## Prerequisites

Before starting this lab, ensure you have:

- **Completed Lab 3** with all verifications passing
- **Working validation** (both real-time and full-form)
- **Success messages and form reset** working via `HX-Trigger`
- **Fragment helpers** (`IsHtmx()` and `Fragment()`) in place

---

## Step 1: Create the Task List View Model (5–7 minutes)

To support filtering and pagination, we need a richer view model than just a list of tasks.

### 1.1 Create the TaskListVm Class

**File: `Models/TaskListVm.cs` (create new file)**

```csharp
namespace RazorPagesHtmxWorkshop.Models;

/// <summary>
/// View model for the task list with filtering and pagination support.
/// </summary>
public class TaskListVm
{
    public required IReadOnlyList<TaskItem> Items { get; init; }
    public required int Page { get; init; }
    public required int PageSize { get; init; }
    public required int Total { get; init; }
    public string? Query { get; init; }

    /// <summary>
    /// Total number of pages based on items and page size.
    /// </summary>
    public int TotalPages => (int)Math.Ceiling(Total / (double)PageSize);

    /// <summary>
    /// Whether there's a previous page.
    /// </summary>
    public bool HasPreviousPage => Page > 1;

    /// <summary>
    /// Whether there's a next page.
    /// </summary>
    public bool HasNextPage => Page < TotalPages;
}
```

### 1.2 Understanding the View Model

| Property          | Type                      | Purpose                                 |
|-------------------|---------------------------|-----------------------------------------|
| `Items`           | `IReadOnlyList<TaskItem>` | The current page of tasks               |
| `Page`            | `int`                     | Current page number (1-based)           |
| `PageSize`        | `int`                     | Items per page                          |
| `Total`           | `int`                     | Total number of items (after filtering) |
| `Query`           | `string?`                 | Current filter query                    |
| `TotalPages`      | `int` (computed)          | Total number of pages                   |
| `HasPreviousPage` | `bool` (computed)         | Whether previous page exists            |
| `HasNextPage`     | `bool` (computed)         | Whether next page exists                |

This view model encapsulates all the information needed to render a paginated, filtered list with navigation controls.

---

## Step 2: Add Data Store Methods for Details and Delete (5 minutes)

### 2.1 Add Find and Delete Methods

**File: `Data/InMemoryTaskStore.cs` (add these methods)**

```csharp
/// <summary>
/// Finds a task by ID.
/// </summary>
public static TaskItem? Find(int id) =>
    _tasks.FirstOrDefault(t => t.Id == id);

/// <summary>
/// Deletes a task by ID.
/// Returns true if found and deleted, false if not found.
/// </summary>
public static bool Delete(int id)
{
    var task = _tasks.FirstOrDefault(t => t.Id == id);
    if (task is null) return false;

    _tasks.Remove(task);
    return true;
}
```

These methods enable the Details and Delete features we'll implement.

---

## Step 3: Update the PageModel for Filtering and Pagination (12–15 minutes)

We'll update the PageModel to support URL-based filtering and pagination using model binding.

### 3.1 Add Query Parameter Properties

**File: `Pages/Tasks/Index.cshtml.cs` (add these properties at the top of the class)**

```csharp
[BindProperty(SupportsGet = true)]
public string? Q { get; set; }

[BindProperty(SupportsGet = true)]
public int PageNum { get; set; } = 1;

[BindProperty(SupportsGet = true)]
public int Size { get; set; } = 5;

public string? Query { get; set; }
public int CurrentPage { get; set; } = 1;
public int PageSize { get; set; } = 5;
public int TotalTasks { get; set; }
```

### 3.2 Understanding BindProperty(SupportsGet = true)

```csharp
[BindProperty(SupportsGet = true)]
public string? Q { get; set; }
```

| Aspect             | Detail                                                       |
|--------------------|--------------------------------------------------------------|
| **What it does**   | Binds query string parameters to properties for GET requests |
| **Without it**     | Only POST requests bind to properties                        |
| **With it**        | Both GET and POST requests bind to properties                |
| **Parameter name** | Matches the property name exactly (case-sensitive)           |

This means:
- URL: `/Tasks?Q=test&PageNum=2&Size=10`
- Automatically binds to: `Q = "test"`, `PageNum = 2`, `Size = 10`

### 3.3 Update OnGet Handler

**File: `Pages/Tasks/Index.cshtml.cs` (replace OnGet method)**

```csharp
public IActionResult OnGet()
{
    Query = Q;
    CurrentPage = Math.Max(1, PageNum);
    PageSize = Math.Clamp(Size, 1, 50);

    var all = InMemoryTaskStore.All();

    if (!string.IsNullOrWhiteSpace(Q))
    {
        all = all
            .Where(t => t.Title.Contains(Q, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    TotalTasks = all.Count;
    Tasks = all
        .Skip((CurrentPage - 1) * PageSize)
        .Take(PageSize)
        .ToList();

    if (IsHtmx())
    {
        var vm = new TaskListVm
        {
            Items = Tasks,
            Page = CurrentPage,
            PageSize = PageSize,
            Total = TotalTasks,
            Query = Query
        };
        return Fragment("Partials/_TaskList", vm);
    }

    return Page();
}
```

**Key Changes:**

1. **Filtering**: Applies `Q` parameter to filter tasks
2. **Pagination**: Uses `PageNum` and `Size` to paginate results
3. **htmx support**: Returns fragment for htmx requests, full page otherwise
4. **Validation**: Ensures `PageNum` is at least 1 and `Size` is between 1-50

### 3.4 Add OnGetList Handler

**File: `Pages/Tasks/Index.cshtml.cs` (add this handler)**

```csharp
/// <summary>
/// Returns the task list fragment with optional filtering and pagination.
/// Supports query parameter (Q) for filtering and PageNum/Size for pagination.
/// </summary>
public IActionResult OnGetList()
{
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
```

This handler provides a dedicated endpoint for fetching paginated/filtered lists via htmx.

---

## Step 4: Add Details Handler (5 minutes)

### 4.1 Create OnGetDetails Handler

**File: `Pages/Tasks/Index.cshtml.cs` (add this handler)**

```csharp
/// <summary>
/// Returns the details fragment for a specific task.
/// Called via hx-get from the Details button in each row.
/// </summary>
public IActionResult OnGetDetails(int id)
{
    var task = InMemoryTaskStore.Find(id);
    return Fragment("Partials/_TaskDetails", task);
}
```

This handler fetches a single task and returns a modal fragment.

---

## Step 5: Add Delete Handler (8–10 minutes)

### 5.1 Create OnPostDelete Handler

**File: `Pages/Tasks/Index.cshtml.cs` (add this handler)**

```csharp
/// <summary>
/// Deletes a task and returns the updated list fragment.
/// Uses hx-confirm on the client for confirmation.
///
/// Response behavior:
/// - Success: Returns updated _TaskList + triggers showMessage
/// - Not found: Returns error message to #messages
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

        // Adjust page number if it becomes invalid after deletion
        var totalPages = (int)Math.Ceiling(total / (double)PageSize);
        if (CurrentPage > totalPages && totalPages > 0)
        {
            CurrentPage = totalPages;
        }

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

    FlashMessage = removed ? "Task deleted." : "Task not found.";
    return RedirectToPage();
}
```

**Key Features:**

1. **State preservation**: Applies the same filter/pagination as before delete
2. **Page adjustment**: If deleting the last item on a page, moves back to previous page
3. **Error handling**: Returns error message if task not found
4. **Success messaging**: Triggers `showMessage` event

---

## Step 6: Update OnPostCreate to Return View Model (5 minutes)

The Create handler needs to return the same view model structure as the list.

**File: `Pages/Tasks/Index.cshtml.cs` (update the success path in OnPostCreate)**

```csharp
// Success path in OnPostCreate
InMemoryTaskStore.Add(Input.Title);

if (IsHtmx())
{
    FlashMessage = "Task added successfully!";
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
```

This ensures the list updates correctly after adding a task, preserving filter/pagination state.

---

## Step 7: Create the Task Details Modal Fragment (8–10 minutes)

### 7.1 Create the Details Partial

**File: `Pages/Tasks/Partials/_TaskDetails.cshtml` (create new file)**

```cshtml
@using RazorPagesHtmxWorkshop.Models
@model TaskItem?

@*
    Task Details Modal
    ==================

    Target ID: #task-modal-container
    Purpose: Display detailed information about a single task in a modal
    Model: TaskItem? (null if not found)
*@

<div class="modal fade" id="task-modal" tabindex="-1" aria-labelledby="taskModalLabel" aria-hidden="true">
    <div class="modal-dialog modal-dialog-centered">
        <div class="modal-content workshop-card">
            <div class="modal-header border-bottom border-light">
                <h5 class="modal-title" id="taskModalLabel">Task Details</h5>
                <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
            </div>
            <div class="modal-body">
                @if (Model is null)
                {
                    <div class="text-muted text-center py-4">Task not found.</div>
                }
                else
                {
                    <div class="vstack gap-4 py-2">
                        <div>
                            <label class="form-label text-muted small mb-1">Title</label>
                            <div class="fw-semibold fs-5">@Model.Title</div>
                        </div>

                        <div class="row">
                            <div class="col-6">
                                <label class="form-label text-muted small mb-1">Status</label>
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
                                <label class="form-label text-muted small mb-1">ID</label>
                                <div class="text-muted">@Model.Id</div>
                            </div>
                        </div>

                        <div>
                            <label class="form-label text-muted small mb-1">Created</label>
                            <div>@Model.CreatedUtc.ToLocalTime().ToString("F")</div>
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

### 7.2 Understanding the Modal Pattern

**Key Aspects:**

- **Complete modal structure**: The fragment includes the entire Bootstrap modal (not just the body)
- **Swapped into container**: Gets inserted into `#task-modal-container`
- **JavaScript trigger**: A script shows the modal after it's loaded
- **Null handling**: Gracefully displays a message if task not found

---

## Step 8: Update the Task List Fragment (15–18 minutes)

Now we'll update the task list to use the view model and include pagination controls.

### 8.1 Rewrite _TaskList.cshtml

**File: `Pages/Tasks/Partials/_TaskList.cshtml` (complete rewrite)**

```cshtml
@using RazorPagesHtmxWorkshop.Models
@model TaskListVm

@*
    Task List Fragment with Pagination
    ===================================

    Target ID: #task-list
    Swap: outerHTML
    Returned by: OnGet (htmx), OnGetList, OnPostCreate (on success), OnPostDelete (on success)

    Features:
    - Displays filtered/paginated task list
    - Pagination controls with hx-push-url
    - Delete and Details buttons per row

    Model: TaskListVm (includes items, page info, filter query)
*@

<div id="task-list">
    @if (Model.Items.Count == 0)
    {
        <div class="text-muted py-4 text-center">
            @if (!string.IsNullOrWhiteSpace(Model.Query))
            {
                <p class="mb-2">No tasks match "<strong>@Model.Query</strong>"</p>
                <a href="?"
                   hx-get="?"
                   hx-target="#task-list"
                   hx-swap="outerHTML"
                   hx-push-url="true"
                   class="btn btn-sm btn-outline-secondary">
                    Clear filter
                </a>
            }
            else
            {
                <p class="mb-0">No tasks yet. Add one above!</p>
            }
        </div>
    }
    else
    {
        <ul class="list-group list-group-flush">
            @foreach (var task in Model.Items)
            {
                <li class="list-group-item d-flex justify-content-between align-items-center py-3">
                    <div class="d-flex flex-column">
                        <strong>@task.Title</strong>
                        <span class="small text-muted">
                            Created @task.CreatedUtc.ToLocalTime().ToString("g")
                        </span>
                    </div>

                    <div class="d-flex align-items-center gap-2">
                        @if (task.IsDone)
                        {
                            <span class="badge text-bg-success">Done</span>
                        }
                        else
                        {
                            <span class="badge text-bg-secondary">Open</span>
                        }

                        <div class="btn-group btn-group-sm">
                            @* Details button - loads into modal *@
                            <button type="button"
                                    class="btn btn-outline-secondary"
                                    hx-get="?handler=Details&id=@task.Id"
                                    hx-target="#task-modal-container"
                                    hx-indicator="#task-loading">
                                Details
                            </button>

                            @* Delete button with confirmation *@
                            <button type="button"
                                    class="btn btn-outline-danger"
                                    hx-post="?handler=Delete"
                                    hx-vals='{"id": @task.Id, "Q": "@(Model.Query ?? "")", "PageNum": @Model.Page, "Size": @Model.PageSize}'
                                    hx-confirm="Delete this task? This cannot be undone."
                                    hx-target="#task-list"
                                    hx-swap="outerHTML"
                                    hx-indicator="#task-loading">
                                Delete
                            </button>
                        </div>
                    </div>
                </li>
            }
        </ul>

        @* Pagination Controls *@
        @if (Model.TotalPages > 1)
        {
            <div class="card-footer d-flex justify-content-between align-items-center bg-transparent border-top-0 pt-4">
                <div class="small text-muted">
                    Showing <strong>@((Model.Page - 1) * Model.PageSize + 1)</strong> to
                    <strong>@(Math.Min(Model.Page * Model.PageSize, Model.Total))</strong> of
                    <strong>@Model.Total</strong> tasks
                </div>

                <nav aria-label="Task list pagination">
                    <ul class="pagination pagination-sm mb-0">
                        @* Previous Page *@
                        <li class="page-item @(!Model.HasPreviousPage ? "disabled" : "")">
                            <a hx-get="?Q=@(Model.Query ?? "")&PageNum=@(Model.Page - 1)&Size=@Model.PageSize"
                               hx-target="#task-list"
                               hx-swap="outerHTML"
                               hx-push-url="true"
                               class="page-link"
                               style="cursor: pointer;"
                               aria-label="Previous">
                                <span aria-hidden="true">&laquo;</span>
                            </a>
                        </li>

                        @* Page Numbers *@
                        @for (int i = 1; i <= Model.TotalPages; i++)
                        {
                            <li class="page-item @(i == Model.Page ? "active" : "")" @(i == Model.Page ? "aria-current='page'" : "")>
                                <a class="page-link"
                                   style="cursor: pointer;"
                                   hx-get="?Q=@(Model.Query ?? "")&PageNum=@i&Size=@Model.PageSize"
                                   hx-target="#task-list"
                                   hx-swap="outerHTML"
                                   hx-push-url="true">
                                    @i
                                </a>
                            </li>
                        }

                        @* Next Page *@
                        <li class="page-item @(!Model.HasNextPage ? "disabled" : "")">
                            <a class="page-link"
                               style="cursor: pointer;"
                               hx-get="?Q=@(Model.Query ?? "")&PageNum=@(Model.Page + 1)&Size=@Model.PageSize"
                               hx-target="#task-list"
                               hx-swap="outerHTML"
                               hx-push-url="true"
                               aria-label="Next">
                                <span aria-hidden="true">&raquo;</span>
                            </a>
                        </li>
                    </ul>
                </nav>
            </div>
        }
    }
</div>
```

### 8.2 Understanding Key Patterns

#### Details Button

```html
<button hx-get="?handler=Details&id=@task.Id"
        hx-target="#task-modal-container"
        hx-indicator="#task-loading">
```

| Attribute      | Value                       | Purpose                     |
|----------------|-----------------------------|-----------------------------|
| `hx-get`       | `"?handler=Details&id=..."` | Fetch task details          |
| `hx-target`    | `"#task-modal-container"`   | Inject modal into container |
| `hx-indicator` | `"#task-loading"`           | Show loading spinner        |

#### Delete Button

```html
<button hx-post="?handler=Delete"
        hx-vals='{"id": @task.Id, "Q": "...", "PageNum": @Model.Page, "Size": @Model.PageSize}'
        hx-confirm="Delete this task? This cannot be undone."
        hx-target="#task-list"
        hx-swap="outerHTML">
```

| Attribute    | Value                          | Purpose                  |
|--------------|--------------------------------|--------------------------|
| `hx-post`    | `"?handler=Delete"`            | POST to delete handler   |
| `hx-vals`    | `'{"id": ..., "Q": ..., ...}'` | Pass ID + preserve state |
| `hx-confirm` | `"Delete this task?..."`       | Browser confirmation     |
| `hx-target`  | `"#task-list"`                 | Update the list          |

**Why `hx-vals` includes Q, PageNum, Size:**

These ensure the delete handler can return a list with the same filter/pagination state. Without them, the list would reset to page 1 with no filter.

#### Pagination Links

```html
<a hx-get="?Q=@(Model.Query ?? "")&PageNum=@i&Size=@Model.PageSize"
   hx-target="#task-list"
   hx-swap="outerHTML"
   hx-push-url="true">
```

| Attribute     | Value                           | Purpose             |
|---------------|---------------------------------|---------------------|
| `hx-get`      | `"?Q=...&PageNum=...&Size=..."` | Fetch specific page |
| `hx-push-url` | `"true"`                        | Update browser URL  |

**URL State Preservation:**

- `Q=@(Model.Query ?? "")` preserves the current filter
- `PageNum=@i` sets the new page
- `Size=@Model.PageSize` maintains page size

---

## Step 9: Update the Main Page for Filtering and Modal (12–15 minutes)

### 9.1 Update Index.cshtml

**File: `Pages/Tasks/Index.cshtml` (update the list section)**

Find the section where the task list is rendered and update it to:

```cshtml
@page
@model RazorPagesHtmxWorkshop.Pages.Tasks.IndexModel
@using RazorPagesHtmxWorkshop.Models
@{
    ViewData["Title"] = "Tasks • htmx Razor Pages Workshop";

    // Build the initial view model for the task list
    var initialVm = new TaskListVm
    {
        Items = Model.Tasks,
        Page = Model.CurrentPage,
        PageSize = Model.PageSize,
        Total = Model.TotalTasks,
        Query = Model.Query
    };
}

<div class="d-flex flex-column flex-md-row align-items-md-end justify-content-between gap-2 mb-3">
    <div>
        <h1 class="mb-1">Tasks</h1>
        <p class="text-muted mb-0">Lab 4: Modals, confirm, history, and pagination.</p>
    </div>
    <div class="d-flex align-items-center gap-2">
        @* Loading indicator - shown during htmx requests *@
        <span id="task-loading" class="htmx-indicator">
            <span class="spinner-border spinner-border-sm text-secondary" role="status" aria-hidden="true"></span>
            <span class="visually-hidden">Loading...</span>
        </span>
        <form method="post" asp-page-handler="Reset" class="m-0">
            <button class="btn btn-sm btn-outline-secondary" type="submit">Reset</button>
        </form>
    </div>
</div>

<partial name="Partials/_Messages" model="Model.FlashMessage" />

<div class="row g-3 g-md-4">
    <div class="col-lg-5">
        <div class="card workshop-card h-100">
            <div class="card-body">
                <div class="workshop-kicker">Fragment boundary</div>
                <h2 class="h5">Create</h2>
                <partial name="Partials/_TaskForm" model="Model" />
            </div>
        </div>
    </div>

    <div class="col-lg-7">
        <div class="card workshop-card h-100">
            <div class="card-body">
                <div class="workshop-kicker">Fragment boundary</div>
                <h2 class="h5 mb-3">List</h2>

                @*
                    Filter Input
                    ============

                    Filters the task list as user types (debounced).
                    Updates URL via hx-push-url for bookmarkability.
                *@
                <div class="mb-3">
                    <div class="input-group">
                        <span class="input-group-text">
                            <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" fill="currentColor" viewBox="0 0 16 16">
                                <path d="M11.742 10.344a6.5 6.5 0 1 0-1.397 1.398h-.001c.03.04.062.078.098.115l3.85 3.85a1 1 0 0 0 1.415-1.414l-3.85-3.85a1.007 1.007 0 0 0-.115-.1zM12 6.5a5.5 5.5 0 1 1-11 0 5.5 5.5 0 0 1 11 0z"/>
                            </svg>
                        </span>
                        <input type="text"
                               class="form-control"
                               name="Q"
                               placeholder="Filter tasks..."
                               value="@Model.Query"
                               hx-get="?"
                               hx-trigger="keyup changed delay:400ms"
                               hx-target="#task-list"
                               hx-swap="outerHTML"
                               hx-push-url="true"
                               hx-indicator="#task-loading" />
                    </div>
                </div>

                <partial name="Partials/_TaskList" model="initialVm" />
            </div>
        </div>
    </div>
</div>

@*
    Details Modal Placeholder
    =========================

    Target ID: #task-modal-container
    Purpose: Hosts the dynamic modal content
*@
<div id="task-modal-container"></div>

@* Listener: Refresh messages when showMessage event fires *@
<div hx-get="?handler=Messages"
     hx-trigger="showMessage from:body"
     hx-target="#messages"
     hx-swap="outerHTML">
</div>

@* Listener: Reset form when clearForm event fires *@
<div hx-get="?handler=EmptyForm"
     hx-trigger="clearForm from:body"
     hx-target="#task-form"
     hx-swap="outerHTML">
</div>

<script>
    document.addEventListener('htmx:afterOnLoad', function (evt) {
        if (evt.detail.target.id === 'task-modal-container') {
            const modalEl = document.getElementById('task-modal');
            if (modalEl) {
                const modal = new bootstrap.Modal(modalEl);
                modal.show();
            }
        }
    });
</script>

@section Scripts {
    <partial name="_ValidationScriptsPartial" />
}
```

### 9.2 Understanding Key Elements

#### Filter Input

```html
<input name="Q"
       hx-get="?"
       hx-trigger="keyup changed delay:400ms"
       hx-push-url="true" />
```

| Attribute     | Value                         | Purpose                           |
|---------------|-------------------------------|-----------------------------------|
| `name`        | `"Q"`                         | Parameter name for binding        |
| `hx-get`      | `"?"`                         | GET to current page (calls OnGet) |
| `hx-trigger`  | `"keyup changed delay:400ms"` | Debounced keystroke               |
| `hx-push-url` | `"true"`                      | Update browser URL                |

**Why `hx-get="?"` instead of `?handler=List`:**

Using `?` calls the default `OnGet` handler, which checks `IsHtmx()` and returns a fragment. This is simpler and more consistent.

#### Modal Container

```html
<div id="task-modal-container"></div>
```

This empty div serves as the injection point for the modal. The modal is dynamically loaded and removed.

#### Modal Show Script

```javascript
document.addEventListener('htmx:afterOnLoad', function (evt) {
    if (evt.detail.target.id === 'task-modal-container') {
        const modalEl = document.getElementById('task-modal');
        if (modalEl) {
            const modal = new bootstrap.Modal(modalEl);
            modal.show();
        }
    }
});
```

**How it works:**

1. htmx fires `htmx:afterOnLoad` after swapping content
2. Script checks if the target was `#task-modal-container`
3. If yes, finds the `#task-modal` element (now in the DOM)
4. Creates a Bootstrap Modal instance and shows it

---

## Step 10: Add CSS Transitions (Optional, 5–7 minutes)

Add visual polish with CSS transitions.

### 10.1 Add Transition Styles

**File: `wwwroot/css/site.css` (add these styles)**

```css
/* ==========================================================================
   htmx Transitions
   ========================================================================== */

/*
 * Loading indicator visibility
 * Show/hide elements with .htmx-indicator class during requests
 */
.htmx-indicator {
    display: none;
}

.htmx-request .htmx-indicator,
.htmx-request.htmx-indicator {
    display: inline-block;
}

/*
 * Fade transition for task list
 * Smoothly fade out old content and fade in new content
 */
#task-list {
    transition: opacity 150ms ease-in-out;
}

#task-list.htmx-swapping {
    opacity: 0;
}

#task-list.htmx-settling {
    opacity: 1;
}

/*
 * Subtle highlight for newly added items
 */
.list-group-item.htmx-added {
    animation: highlight-fade 1s ease-out;
}

@keyframes highlight-fade {
    from {
        background-color: rgba(var(--bs-success-rgb), 0.2);
    }
    to {
        background-color: transparent;
    }
}

/*
 * Disabled state during requests
 * Prevent double-clicks by visually disabling buttons
 */
button.htmx-request {
    opacity: 0.6;
    pointer-events: none;
}
```

### 10.2 Understanding htmx CSS Classes

htmx adds these classes during the swap lifecycle:

| Class           | When Applied              | Duration       |
|-----------------|---------------------------|----------------|
| `htmx-request`  | Request is in flight      | Until response |
| `htmx-swapping` | Old content being removed | Brief          |
| `htmx-settling` | New content settling in   | Brief          |
| `htmx-added`    | New elements just added   | Brief          |

---

## Complete Code Reference

### Index.cshtml.cs (Complete PageModel)

**File: `Pages/Tasks/Index.cshtml.cs`**

```csharp
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

    [BindProperty(SupportsGet = true)]
    public string? Q { get; set; }

    [BindProperty(SupportsGet = true)]
    public int PageNum { get; set; } = 1;

    [BindProperty(SupportsGet = true)]
    public int Size { get; set; } = 5;

    public string? Query { get; set; }
    public int CurrentPage { get; set; } = 1;
    public int PageSize { get; set; } = 5;
    public int TotalTasks { get; set; }

    #region Helper Methods

    private bool IsHtmx() =>
        Request.Headers.TryGetValue("HX-Request", out var value) && value == "true";

    private PartialViewResult Fragment(string partialName, object model) =>
        new()
        {
            ViewName = partialName,
            ViewData = new ViewDataDictionary(MetadataProvider, ModelState) { Model = model }
        };

    #endregion

    #region Page Handlers

    public IActionResult OnGet()
    {
        Query = Q;
        CurrentPage = Math.Max(1, PageNum);
        PageSize = Math.Clamp(Size, 1, 50);

        var all = InMemoryTaskStore.All();

        if (!string.IsNullOrWhiteSpace(Q))
        {
            all = all
                .Where(t => t.Title.Contains(Q, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        TotalTasks = all.Count;
        Tasks = all
            .Skip((CurrentPage - 1) * PageSize)
            .Take(PageSize)
            .ToList();

        if (IsHtmx())
        {
            var vm = new TaskListVm
            {
                Items = Tasks,
                Page = CurrentPage,
                PageSize = PageSize,
                Total = TotalTasks,
                Query = Query
            };
            return Fragment("Partials/_TaskList", vm);
        }

        return Page();
    }

    public IActionResult OnGetList()
    {
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

    public IActionResult OnGetDetails(int id)
    {
        var task = InMemoryTaskStore.Find(id);
        return Fragment("Partials/_TaskDetails", task);
    }

    public IActionResult OnGetMessages()
    {
        return Fragment("Partials/_Messages", FlashMessage);
    }

    public IActionResult OnGetEmptyForm()
    {
        Input = new NewTaskInput();
        ModelState.Clear();
        return Fragment("Partials/_TaskForm", this);
    }

    #endregion

    #region Validation Handlers

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

    #region Action Handlers

    public IActionResult OnPostCreate()
    {
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

        InMemoryTaskStore.Add(Input.Title);

        if (IsHtmx())
        {
            FlashMessage = "Task added successfully!";
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

            // Adjust page number if it becomes invalid after deletion
            var totalPages = (int)Math.Ceiling(total / (double)PageSize);
            if (CurrentPage > totalPages && totalPages > 0)
            {
                CurrentPage = totalPages;
            }

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
    }

    #endregion
}
```

### InMemoryTaskStore.cs (Updated)

**File: `Data/InMemoryTaskStore.cs`**

```csharp
using RazorPagesHtmxWorkshop.Models;

namespace RazorPagesHtmxWorkshop.Data;

public static class InMemoryTaskStore
{
    private static int _nextId = 1;
    private static readonly List<TaskItem> _tasks = new();

    public static IReadOnlyList<TaskItem> All() =>
        _tasks.OrderByDescending(t => t.CreatedUtc).ToList();

    /// <summary>
    /// Finds a task by ID.
    /// </summary>
    public static TaskItem? Find(int id) =>
        _tasks.FirstOrDefault(t => t.Id == id);

    public static TaskItem Add(string title)
    {
        var item = new TaskItem
        {
            Id = _nextId++,
            Title = title.Trim(),
            IsDone = false,
            CreatedUtc = DateTime.UtcNow
        };

        _tasks.Add(item);
        return item;
    }

    /// <summary>
    /// Deletes a task by ID.
    /// Returns true if found and deleted, false if not found.
    /// </summary>
    public static bool Delete(int id)
    {
        var task = _tasks.FirstOrDefault(t => t.Id == id);
        if (task is null) return false;

        _tasks.Remove(task);
        return true;
    }

    /// <summary>
    /// Useful for workshops / resetting between labs
    /// </summary>
    public static void Reset()
    {
        _tasks.Clear();
        _nextId = 1;
    }
}
```

---

## Verification Checklist

Before moving to Lab 5, verify these behaviors:

### Details Pattern

- [ ] Clicking "Details" opens a Bootstrap modal
- [ ] Modal displays task information correctly
- [ ] Closing modal works (X button or backdrop click)
- [ ] Modal shows "Task not found" for invalid IDs

### Delete Pattern

- [ ] Clicking "Delete" shows browser confirmation dialog
- [ ] Confirming deletes task and updates list
- [ ] Canceling keeps task in list
- [ ] Success message appears after delete
- [ ] List refreshes with correct pagination after delete

### Filtering

- [ ] Typing in filter updates list after 400ms
- [ ] Filter is applied (shows matching tasks only)
- [ ] URL updates with Q parameter (`?Q=test`)
- [ ] "Clear filter" link removes filter and resets URL
- [ ] Filter persists when navigating pages

### Pagination

- [ ] Pagination controls appear when tasks > page size (5)
- [ ] Clicking page number updates list
- [ ] URL updates with PageNum parameter
- [ ] Browser back/forward navigates through history
- [ ] Filter is preserved when changing pages

### State Preservation

- [ ] Deleting a task preserves filter and pagination
- [ ] Adding a task preserves filter and pagination
- [ ] Refreshing the page loads with correct filter/pagination

### Transitions

- [ ] List fades during swap (if CSS added)
- [ ] Loading indicator appears during requests
- [ ] Buttons are disabled during their request

---

## Key Takeaways

### Patterns Summary

| Pattern           | Key Attributes                                | Purpose                       |
|-------------------|-----------------------------------------------|-------------------------------|
| **Details Modal** | `hx-get`, `hx-target="#task-modal-container"` | Load content into modal       |
| **Delete**        | `hx-post`, `hx-confirm`, `hx-vals`            | Safe destructive actions      |
| **Filter**        | `hx-get="?"`, `hx-push-url`                   | Live search with URL state    |
| **Pagination**    | `hx-push-url` on links                        | Navigable, bookmarkable pages |
| **Transitions**   | CSS + htmx classes                            | Visual polish                 |

### URL State Philosophy

The key insight of this lab is that **URLs should reflect application state**:

- Filter query → `?Q=test`
- Current page → `?PageNum=2`
- Page size → `?Size=10`

This enables:

- **Bookmarking**: Users save filtered/paginated views
- **Sharing**: Send a link to a specific view
- **History**: Back/forward buttons work correctly
- **Progressive enhancement**: Non-JS users get the same functionality

### Model Binding with BindProperty

```csharp
[BindProperty(SupportsGet = true)]
public string? Q { get; set; }
```

This is a cleaner alternative to manually reading query parameters:

| Without BindProperty | With BindProperty |
|----------------------|-------------------|
| `Request.Query["q"]` | `Q` property      |
| Manual parsing       | Automatic binding |
| Easy to forget       | Declarative       |

### State Preservation Pattern

When an action might change the list (create, delete), preserve the current state:

```csharp
hx-vals='{"id": @task.Id, "Q": "@(Model.Query ?? "")", "PageNum": @Model.Page, "Size": @Model.PageSize}'
```

This ensures users don't lose their place when performing actions.

---

## Troubleshooting

### Common Issues

| Problem                  | Likely Cause                   | Solution                                      |
|--------------------------|--------------------------------|-----------------------------------------------|
| Modal doesn't open       | Missing Bootstrap JS or script | Check Bootstrap is loaded, verify script      |
| Modal doesn't close      | Missing data-bs-dismiss        | Add `data-bs-dismiss="modal"` to close button |
| URL doesn't update       | Missing `hx-push-url`          | Add `hx-push-url="true"`                      |
| Back button doesn't work | OnGet doesn't handle params    | Ensure OnGet uses Q, PageNum, Size            |
| Filter resets on delete  | Missing Q in hx-vals           | Include Q in delete button's hx-vals          |
| Page shows wrong items   | Binding case mismatch          | Use exact names: Q, PageNum, Size             |

### Debug Tips

1. **Check Network tab**: Verify request URL includes all parameters
2. **Check Response**: Confirm fragment structure is correct
3. **Check URL bar**: Verify `hx-push-url` is updating the URL
4. **Check Console**: Look for htmx or Bootstrap errors
5. **Test back/forward**: Ensure page loads with correct state
6. **Inspect htmx:afterOnLoad**: Verify modal script is firing

---

## What Comes Next

In **Lab 5**, you'll implement:

- Dynamic form rows (Add/Remove tag inputs)
- Dependent dropdowns (Category → Subcategory)
- Long-running operations with polling
- Out-of-band swaps for global updates

**Proceed to Lab 5: Dynamic Forms + Long-Running UX (Polling) →**
