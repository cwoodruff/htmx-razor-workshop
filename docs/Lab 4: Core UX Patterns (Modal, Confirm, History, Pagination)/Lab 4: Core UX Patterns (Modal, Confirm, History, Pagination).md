---
order: 1
icon: code-square
---

# Lab 4: Core UX Patterns (Modal, Confirm, History, Pagination)

## Overview

In this lab, you'll implement the core UX patterns that make htmx-powered applications feel polished and professional. These patterns are the building blocks of real-world interactive applications:

- **Details View**: Load content into a panel or modal without page reload
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

| Outcome                 | Description                                              |
|-------------------------|----------------------------------------------------------|
| **Details pattern**     | Load task details into a side panel or Bootstrap modal   |
| **Delete with confirm** | Use `hx-confirm` for safe destructive actions            |
| **Filtering**           | Filter the task list with URL state preserved            |
| **Pagination**          | Navigate pages with back/forward support                 |
| **URL state**           | Use `hx-push-url` to make URLs bookmarkable              |
| **Swap strategies**     | Choose between `outerHTML` and `innerHTML` appropriately |
| **Transitions**         | Add CSS transitions for smooth visual feedback           |

---

## Prerequisites

Before starting this lab, ensure you have:

- **Completed Lab 3** with all verifications passing
- **Working validation** (both real-time and full-form)
- **Success messages and form reset** working via `HX-Trigger`
- **Fragment helpers** (`IsHtmx()` and `Fragment()`) in place

---

## Pattern 1: Details View (10–15 minutes)

The Details pattern loads additional information about an item without navigating away from the list. You can implement this as either a **side panel** (simpler) or a **Bootstrap modal** (more polished).

### 1.1 Choose Your Implementation

| Option              | Complexity | Best For                       |
|---------------------|------------|--------------------------------|
| **Side Panel**      | Simple     | Always-visible detail area     |
| **Bootstrap Modal** | Moderate   | Focused attention, familiar UX |

We'll cover both options. Choose one to implement.

---

### Option A: Side Panel (Simplest Approach)

#### A.1 Add the Details Container

Add a details panel to `Pages/Tasks/Index.cshtml`:

**File: `Pages/Tasks/Index.cshtml` (add in the right column or below the list)**

```cshtml
@*
    Details Panel
    =============

    Fragment boundary: #task-details
    Purpose: Display detailed information about a selected task

    Design notes:
    - Always renders the wrapper for consistent swapping
    - Default state shows placeholder text
    - Loaded via hx-get from task row buttons
*@

<div class="card mt-4">
    <div class="card-header">
        <h5 class="mb-0">Task Details</h5>
    </div>
    <div class="card-body">
        <div id="task-details" class="text-muted">
            Select a task to view details.
        </div>
    </div>
</div>
```

#### A.2 Create the Details Fragment

**File: `Pages/Tasks/Partials/_TaskDetails.cshtml`**

```cshtml
@using RazorHtmxWorkshop.Models
@model TaskItem?

@*
    Task Details Fragment
    =====================

    Purpose: Display detailed information about a single task
    Model: TaskItem? (null if not found)

    Design notes:
    - Wrapper div always renders (stable target for htmx)
    - Handles null case gracefully
    - Shows all task properties
*@

<div id="task-details">
    @if (Model is null)
    {
        <div class="text-muted">Task not found.</div>
    }
    else
    {
        <div class="vstack gap-3">
            <div>
                <label class="form-label text-muted small mb-0">Title</label>
                <div class="fw-semibold">@Model.Title</div>
            </div>

            <div>
                <label class="form-label text-muted small mb-0">Status</label>
                <div>
                    @if (Model.IsDone)
                    {
                        <span class="badge bg-success">Done</span>
                    }
                    else
                    {
                        <span class="badge bg-secondary">Open</span>
                    }
                </div>
            </div>

            <div>
                <label class="form-label text-muted small mb-0">Created</label>
                <div>@Model.CreatedUtc.ToLocalTime().ToString("f")</div>
            </div>

            <div>
                <label class="form-label text-muted small mb-0">ID</label>
                <div class="text-muted small">@Model.Id</div>
            </div>
        </div>
    }
</div>
```

#### A.3 Add the Details Handler

First, add a `Find` method to your data store:

**File: `Data/InMemoryTaskStore.cs` (add this method)**

```csharp
/// <summary>
/// Finds a task by ID.
/// </summary>
public static TaskItem? Find(int id) =>
    _tasks.FirstOrDefault(t => t.Id == id);
```

Now add the handler:

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

#### A.4 Add Details Button to Each Task Row

Update `_TaskList.cshtml` to include a Details button:

**File: `Pages/Tasks/Partials/_TaskList.cshtml` (update the foreach loop)**

```cshtml
@using RazorHtmxWorkshop.Models
@model IReadOnlyList<TaskItem>

<div id="task-list">
    @if (Model.Count == 0)
    {
        <p class="text-muted">No tasks yet.</p>
    }
    else
    {
        <ul class="list-group">
            @foreach (var task in Model)
            {
                <li class="list-group-item d-flex justify-content-between align-items-center">
                    <div>
                        <span class="fw-medium">@task.Title</span>
                        <span class="text-muted small ms-2">
                            @task.CreatedUtc.ToLocalTime().ToString("g")
                        </span>
                    </div>

                    <div class="btn-group btn-group-sm">
                        @* Details button - loads into side panel *@
                        <button type="button"
                                class="btn btn-outline-secondary"
                                hx-get="?handler=Details&id=@task.Id"
                                hx-target="#task-details"
                                hx-swap="outerHTML">
                            Details
                        </button>

                        @if (task.IsDone)
                        {
                            <span class="btn btn-outline-success disabled">Done</span>
                        }
                    </div>
                </li>
            }
        </ul>
    }
</div>
```

#### A.5 Understanding the Pattern

```html
<button hx-get="?handler=Details&id=@task.Id"
        hx-target="#task-details"
        hx-swap="outerHTML">
```

| Attribute   | Value                            | Purpose                  |
|-------------|----------------------------------|--------------------------|
| `hx-get`    | `"?handler=Details&id=@task.Id"` | GET request with task ID |
| `hx-target` | `"#task-details"`                | Update the details panel |
| `hx-swap`   | `"outerHTML"`                    | Replace entire fragment  |

**The Flow:**

1. User clicks "Details" button
2. htmx sends GET to `?handler=Details&id=123`
3. Server returns `_TaskDetails` fragment
4. htmx swaps `#task-details` with response

---

### Option B: Bootstrap Modal (More Polished)

#### B.1 Add the Modal Shell

Add a Bootstrap modal to `Pages/Tasks/Index.cshtml`:

**File: `Pages/Tasks/Index.cshtml` (add before closing the main content)**

```cshtml
@*
    Details Modal
    =============

    Bootstrap modal shell - content loaded dynamically via htmx
    The modal body (#details-modal-body) is the swap target

    Design notes:
    - Modal structure is static (not swapped)
    - Only modal-body content is dynamic
    - Uses innerHTML swap since we're filling the body
*@

<div class="modal fade" id="detailsModal" tabindex="-1" aria-labelledby="detailsModalLabel" aria-hidden="true">
    <div class="modal-dialog modal-dialog-centered">
        <div class="modal-content">
            <div class="modal-header">
                <h5 class="modal-title" id="detailsModalLabel">Task Details</h5>
                <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
            </div>
            <div class="modal-body" id="details-modal-body">
                <div class="text-muted">Loading...</div>
            </div>
            <div class="modal-footer">
                <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Close</button>
            </div>
        </div>
    </div>
</div>
```

#### B.2 Create the Modal Body Fragment

**File: `Pages/Tasks/Partials/_TaskDetailsModal.cshtml`**

```cshtml
@using RazorHtmxWorkshop.Models
@model TaskItem?

@*
    Task Details Modal Body Fragment
    ================================

    Purpose: Content for the details modal body
    Model: TaskItem? (null if not found)

    Note: This is the CONTENT only - not the modal wrapper.
    Uses innerHTML swap into #details-modal-body.
*@

@if (Model is null)
{
    <div class="text-muted">Task not found.</div>
}
else
{
    <div class="vstack gap-3">
        <div>
            <label class="form-label text-muted small mb-0">Title</label>
            <div class="fw-semibold fs-5">@Model.Title</div>
        </div>

        <div class="row">
            <div class="col-6">
                <label class="form-label text-muted small mb-0">Status</label>
                <div>
                    @if (Model.IsDone)
                    {
                        <span class="badge bg-success">Done</span>
                    }
                    else
                    {
                        <span class="badge bg-secondary">Open</span>
                    }
                </div>
            </div>
            <div class="col-6">
                <label class="form-label text-muted small mb-0">ID</label>
                <div class="text-muted">@Model.Id</div>
            </div>
        </div>

        <div>
            <label class="form-label text-muted small mb-0">Created</label>
            <div>@Model.CreatedUtc.ToLocalTime().ToString("F")</div>
        </div>
    </div>
}
```

#### B.3 Update the Handler for Modal

**File: `Pages/Tasks/Index.cshtml.cs` (update or add)**

```csharp
/// <summary>
/// Returns the details fragment for a specific task.
/// For modal: returns content only (innerHTML swap)
/// For panel: returns wrapper (outerHTML swap)
/// </summary>
public IActionResult OnGetDetails(int id)
{
    var task = InMemoryTaskStore.Find(id);

    // Use modal version (no wrapper needed for innerHTML)
    return Fragment("Partials/_TaskDetailsModal", task);
}
```

#### B.4 Update the Details Button for Modal

**File: `Pages/Tasks/Partials/_TaskList.cshtml` (update button)**

```cshtml
@* Details button - loads into modal and shows it *@
<button type="button"
        class="btn btn-outline-secondary"
        hx-get="?handler=Details&id=@task.Id"
        hx-target="#details-modal-body"
        hx-swap="innerHTML"
        hx-on::after-request="bootstrap.Modal.getOrCreateInstance(document.getElementById('detailsModal')).show()">
    Details
</button>
```

#### B.5 Understanding the Modal Pattern

| Attribute              | Value                   | Purpose                                    |
|------------------------|-------------------------|--------------------------------------------|
| `hx-target`            | `"#details-modal-body"` | Target the modal body, not the whole modal |
| `hx-swap`              | `"innerHTML"`           | Replace body content only                  |
| `hx-on::after-request` | Bootstrap modal show    | Open modal after content loads             |

**Key Difference from Panel:**

- **Panel**: `outerHTML` swap replaces the entire `#task-details` div
- **Modal**: `innerHTML` swap fills the `#details-modal-body` container

**Why `innerHTML` for Modal?**

The modal structure (header, footer, close button) is static. We only want to replace the body content, not the entire modal.

---

## Pattern 2: Delete with Confirmation (10–12 minutes)

Destructive actions need confirmation. htmx provides `hx-confirm` for simple browser dialogs, or you can build custom confirmation UIs.

### 2.1 Add Delete Method to Data Store

**File: `Data/InMemoryTaskStore.cs` (add this method)**

```csharp
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

### 2.2 Add the Delete Handler

**File: `Pages/Tasks/Index.cshtml.cs` (add this handler)**

```csharp
/// <summary>
/// Deletes a task and returns the updated list fragment.
/// Uses hx-confirm on the client for confirmation.
///
/// Response behavior:
/// - Success: Returns updated _TaskList + triggers showMessage
/// - Not found: Returns 404 + error message to #messages
/// </summary>
public IActionResult OnPostDelete(int id)
{
    var removed = InMemoryTaskStore.Delete(id);
    Tasks = InMemoryTaskStore.All();

    if (IsHtmx())
    {
        if (!removed)
        {
            // Task was already deleted or never existed
            Response.Headers["HX-Retarget"] = "#messages";
            Response.Headers["HX-Reswap"] = "outerHTML";
            return Fragment("Partials/_Messages", "Task not found (already deleted?).");
        }

        // Success: update list and show message
        FlashMessage = "Task deleted.";
        Response.Headers["HX-Trigger"] = "showMessage";
        return Fragment("Partials/_TaskList", Tasks);
    }

    // Non-htmx fallback
    FlashMessage = removed ? "Task deleted." : "Task not found.";
    return RedirectToPage();
}
```

### 2.3 Add Delete Button to Each Row

Update `_TaskList.cshtml` to include a Delete button:

**File: `Pages/Tasks/Partials/_TaskList.cshtml` (add to button group)**

```cshtml
<div class="btn-group btn-group-sm">
    @* Details button *@
    <button type="button"
            class="btn btn-outline-secondary"
            hx-get="?handler=Details&id=@task.Id"
            hx-target="#task-details"
            hx-swap="outerHTML">
        Details
    </button>

    @* Delete button with confirmation *@
    <button type="button"
            class="btn btn-outline-danger"
            hx-post="?handler=Delete"
            hx-vals='{"id": @task.Id}'
            hx-confirm="Delete this task? This cannot be undone."
            hx-target="#task-list"
            hx-swap="outerHTML">
        Delete
    </button>

    @if (task.IsDone)
    {
        <span class="btn btn-outline-success disabled">Done</span>
    }
</div>
```

### 2.4 Understanding the Delete Pattern

```html
<button hx-post="?handler=Delete"
        hx-vals='{"id": @task.Id}'
        hx-confirm="Delete this task? This cannot be undone."
        hx-target="#task-list"
        hx-swap="outerHTML">
```

| Attribute    | Value                    | Purpose                        |
|--------------|--------------------------|--------------------------------|
| `hx-post`    | `"?handler=Delete"`      | POST request to Delete handler |
| `hx-vals`    | `'{"id": @task.Id}'`     | Pass task ID as JSON           |
| `hx-confirm` | `"Delete this task?..."` | Browser confirmation dialog    |
| `hx-target`  | `"#task-list"`           | Update the entire list         |
| `hx-swap`    | `"outerHTML"`            | Replace list fragment          |

**Why `hx-vals` Instead of Query String?**

You could use `hx-post="?handler=Delete&id=@task.Id"`, but `hx-vals` is cleaner for POST requests:

- Keeps the URL clean
- Sends data in the request body
- Avoids encoding issues with complex values

**The Confirmation Flow:**

1. User clicks Delete
2. Browser shows confirmation dialog (native)
3. If confirmed, htmx sends POST
4. Server deletes task and returns updated list
5. htmx swaps `#task-list`
6. `showMessage` event triggers success message

### 2.5 Handling Edge Cases

The handler covers these scenarios:

| Scenario               | Response    | Status | Target                   |
|------------------------|-------------|--------|--------------------------|
| Task found and deleted | `_TaskList` | 200    | `#task-list`             |
| Task not found         | `_Messages` | 200    | `#messages` (retargeted) |
| Non-htmx request       | Redirect    | 302    | Full page                |

---

## Pattern 3: Filtering with URL State (12–15 minutes)

Filtering should update the list AND the URL, so users can bookmark or share filtered views.

### 3.1 Create a View Model for the List

To support filtering and pagination, we need a richer view model:

**File: `Models/TaskListVm.cs` (create new file)**

```csharp
namespace RazorHtmxWorkshop.Models;

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

### 3.2 Update the List Handler

**File: `Pages/Tasks/Index.cshtml.cs` (update OnGetList)**

```csharp
/// <summary>
/// Returns the task list fragment with optional filtering and pagination.
/// Supports query parameter (q) for filtering and page/pageSize for pagination.
/// </summary>
public IActionResult OnGetList(string? q, int page = 1, int pageSize = 5)
{
    // Validate page parameters
    page = Math.Max(1, page);
    pageSize = Math.Clamp(pageSize, 1, 50);

    // Get all tasks
    var all = InMemoryTaskStore.All();

    // Apply filter if provided
    if (!string.IsNullOrWhiteSpace(q))
    {
        all = all
            .Where(t => t.Title.Contains(q, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    // Calculate pagination
    var total = all.Count;
    var items = all
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .ToList();

    // Build view model
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
```

### 3.3 Update OnGet to Use the Same Logic

**File: `Pages/Tasks/Index.cshtml.cs` (update OnGet)**

```csharp
// Add these properties to hold filter/page state for initial render
public string? Query { get; set; }
public int CurrentPage { get; set; } = 1;
public int PageSize { get; set; } = 5;

public void OnGet(string? q, int page = 1, int pageSize = 5)
{
    // Store for the view
    Query = q;
    CurrentPage = Math.Max(1, page);
    PageSize = Math.Clamp(pageSize, 1, 50);

    // Apply same logic as OnGetList
    var all = InMemoryTaskStore.All();

    if (!string.IsNullOrWhiteSpace(q))
    {
        all = all
            .Where(t => t.Title.Contains(q, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    Tasks = all
        .Skip((CurrentPage - 1) * PageSize)
        .Take(PageSize)
        .ToList();
}
```

### 3.4 Add Filter Input

**File: `Pages/Tasks/Index.cshtml` (add above the task list)**

```cshtml
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
               name="q"
               placeholder="Filter tasks..."
               value="@Model.Query"
               hx-get="?handler=List"
               hx-trigger="keyup changed delay:400ms"
               hx-target="#task-list"
               hx-swap="outerHTML"
               hx-push-url="true" />
    </div>
</div>
```

### 3.5 Understanding `hx-push-url`

```html
<input hx-get="?handler=List"
       hx-trigger="keyup changed delay:400ms"
       hx-push-url="true" />
```

| Attribute     | Value    | Purpose                                 |
|---------------|----------|-----------------------------------------|
| `hx-push-url` | `"true"` | Push the request URL to browser history |

**What This Enables:**

1. User types "buy" in filter
2. htmx sends GET to `?handler=List&q=buy`
3. Browser URL updates to `/Tasks?handler=List&q=buy`
4. User can bookmark or share this URL
5. **Back button** returns to previous filter state

**Why This Matters:**

Without `hx-push-url`, the URL stays static. Users can't bookmark filtered views, and back/forward buttons don't work as expected.

---

## Pattern 4: Pagination with URL State (10–12 minutes)

Now we'll add pagination controls that also update the URL.

### 4.1 Update the List Fragment for Pagination

**File: `Pages/Tasks/Partials/_TaskList.cshtml` (complete rewrite)**

```cshtml
@using RazorHtmxWorkshop.Models
@model TaskListVm

@*
    Task List Fragment with Pagination
    ===================================

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
                <p>No tasks match "<strong>@Model.Query</strong>"</p>
                <a href="?handler=List"
                   hx-get="?handler=List"
                   hx-target="#task-list"
                   hx-swap="outerHTML"
                   hx-push-url="true"
                   class="btn btn-sm btn-outline-secondary">
                    Clear filter
                </a>
            }
            else
            {
                <p>No tasks yet. Add one above!</p>
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

                    <div class="btn-group btn-group-sm">
                        <button type="button"
                                class="btn btn-outline-secondary"
                                hx-get="?handler=Details&id=@task.Id"
                                hx-target="#task-details"
                                hx-swap="outerHTML">
                            Details
                        </button>

                        <button type="button"
                                class="btn btn-outline-danger"
                                hx-post="?handler=Delete"
                                hx-vals='{"id": @task.Id}'
                                hx-confirm="Delete this task? This cannot be undone."
                                hx-target="#task-list"
                                hx-swap="outerHTML">
                            Delete
                        </button>
                    </div>
                </li>
            }
        </ul>

        @* Pagination Controls *@
        @if (Model.TotalPages > 1)
        {
            <nav class="mt-3" aria-label="Task list pagination">
                <ul class="pagination pagination-sm justify-content-center mb-0">
                    @* Previous button *@
                    <li class="page-item @(Model.HasPreviousPage ? "" : "disabled")">
                        <a class="page-link"
                           hx-get="?handler=List&q=@Model.Query&page=@(Model.Page - 1)&pageSize=@Model.PageSize"
                           hx-target="#task-list"
                           hx-swap="outerHTML"
                           hx-push-url="true"
                           @(Model.HasPreviousPage ? "" : "tabindex=\"-1\" aria-disabled=\"true\"")>
                            Previous
                        </a>
                    </li>

                    @* Page numbers *@
                    @for (var p = 1; p <= Model.TotalPages; p++)
                    {
                        var isActive = p == Model.Page;
                        <li class="page-item @(isActive ? "active" : "")">
                            <a class="page-link"
                               hx-get="?handler=List&q=@Model.Query&page=@p&pageSize=@Model.PageSize"
                               hx-target="#task-list"
                               hx-swap="outerHTML"
                               hx-push-url="true"
                               @(isActive ? "aria-current=\"page\"" : "")>
                                @p
                            </a>
                        </li>
                    }

                    @* Next button *@
                    <li class="page-item @(Model.HasNextPage ? "" : "disabled")">
                        <a class="page-link"
                           hx-get="?handler=List&q=@Model.Query&page=@(Model.Page + 1)&pageSize=@Model.PageSize"
                           hx-target="#task-list"
                           hx-swap="outerHTML"
                           hx-push-url="true"
                           @(Model.HasNextPage ? "" : "tabindex=\"-1\" aria-disabled=\"true\"")>
                            Next
                        </a>
                    </li>
                </ul>
            </nav>

            <div class="text-center text-muted small mt-2">
                Showing @((Model.Page - 1) * Model.PageSize + 1)–@(Math.Min(Model.Page * Model.PageSize, Model.Total)) of @Model.Total tasks
            </div>
        }
    }
</div>
```

### 4.2 Understanding the Pagination Pattern

Each pagination link includes:

```html
<a hx-get="?handler=List&q=@Model.Query&page=@p&pageSize=@Model.PageSize"
   hx-target="#task-list"
   hx-swap="outerHTML"
   hx-push-url="true">
```

**URL State Preservation:**

- `q=@Model.Query` preserves the current filter
- `page=@p` sets the new page
- `pageSize=@Model.PageSize` maintains page size
- `hx-push-url="true"` updates the browser URL

**Result:**

- URL becomes `/Tasks?handler=List&q=buy&page=2&pageSize=5`
- User can bookmark this exact view
- Back/forward navigates through filter/page history

### 4.3 Update the Page to Use the View Model

Update the list partial call in `Index.cshtml` to handle both the simple model (for form submissions) and the view model (for filtered/paginated views).

Since `OnPostCreate` still returns the simple list, you'll need to update it:

**File: `Pages/Tasks/Index.cshtml.cs` (update OnPostCreate success path)**

```csharp
// Success path in OnPostCreate
InMemoryTaskStore.Add(Input.Title);

if (IsHtmx())
{
    FlashMessage = "Task added successfully!";
    Response.Headers["HX-Trigger"] = "showMessage,clearForm";

    // Return a view model with default pagination
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
```

---

## Pattern 5: Smooth Transitions (5–7 minutes)

Add CSS transitions to make swaps feel polished rather than jarring.

### 5.1 Understanding htmx CSS Classes

htmx adds these classes during the swap lifecycle:

| Class           | When Applied              | Duration       |
|-----------------|---------------------------|----------------|
| `htmx-request`  | Request is in flight      | Until response |
| `htmx-swapping` | Old content being removed | Brief          |
| `htmx-settling` | New content settling in   | Brief          |
| `htmx-added`    | New elements just added   | Brief          |

### 5.2 Add Transition Styles

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
 * Fade transition for details panel
 */
#task-details {
    transition: opacity 150ms ease-in-out;
}

#task-details.htmx-swapping {
    opacity: 0;
}

#task-details.htmx-settling {
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

### 5.3 Add a Loading Spinner

Update the form and buttons to show a loading indicator:

**File: `Pages/Tasks/Index.cshtml` (add near the toolbar)**

```cshtml
@* Loading indicator - shown during htmx requests *@
<span id="task-loading" class="htmx-indicator">
    <span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span>
    <span class="visually-hidden">Loading...</span>
</span>
```

Update elements to reference the indicator:

```html
<!-- On the filter input -->
<input ... hx-indicator="#task-loading" />

<!-- On the form -->
<form ... hx-indicator="#task-loading">
```

### 5.4 Swap Timing Modifiers (Optional)

You can fine-tune swap timing with modifiers:

```html
hx-swap="outerHTML swap:100ms settle:200ms"
```

| Modifier  | Default | Purpose                                   |
|-----------|---------|-------------------------------------------|
| `swap:`   | 0ms     | Delay before removing old content         |
| `settle:` | 20ms    | Delay before settling classes are removed |

**Example: Slower, More Visible Transition**

```html
<button hx-get="?handler=Details&id=@task.Id"
        hx-target="#task-details"
        hx-swap="outerHTML swap:50ms settle:150ms">
    Details
</button>
```

---

## Pattern 6: Swap Strategy Guide (Reference)

Choose the right swap strategy for each use case:

### 6.1 Swap Strategy Reference

| Strategy      | What It Does              | Use When                                 |
|---------------|---------------------------|------------------------------------------|
| `innerHTML`   | Replace target's children | Filling a container (modal body)         |
| `outerHTML`   | Replace entire target     | Replacing a fragment with wrapper        |
| `beforebegin` | Insert before target      | Adding to a list (prepend sibling)       |
| `afterbegin`  | Insert as first child     | Adding to a list (prepend child)         |
| `beforeend`   | Insert as last child      | Adding to a list (append child)          |
| `afterend`    | Insert after target       | Adding to a list (append sibling)        |
| `delete`      | Remove target             | Removing an element                      |
| `none`        | Don't swap                | Side effects only (e.g., trigger events) |

### 6.2 When to Use Each

**`outerHTML` (Most Common)**

```html
<!-- Fragment includes wrapper with ID -->
<div id="task-list">...</div>

<!-- Use outerHTML to replace it -->
hx-swap="outerHTML"
```

**`innerHTML`**

```html
<!-- Container stays, content changes -->
<div class="modal-body" id="details-modal-body">
    <!-- This content gets replaced -->
</div>

<!-- Use innerHTML to fill it -->
hx-swap="innerHTML"
```

**`beforeend` (Adding Items)**

```html
<!-- Add new item to end of list -->
<ul id="task-items">
    <li>Existing item</li>
    <!-- New items go here -->
</ul>

hx-swap="beforeend"
hx-target="#task-items"
```

---

## Complete Code Reference

### Index.cshtml.cs (Complete)

**File: `Pages/Tasks/Index.cshtml.cs`**

```csharp
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using RazorHtmxWorkshop.Data;
using RazorHtmxWorkshop.Models;

namespace RazorHtmxWorkshop.Pages.Tasks;

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

    private bool IsHtmx() =>
        Request.Headers.TryGetValue("HX-Request", out var value) && value == "true";

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
using RazorHtmxWorkshop.Models;

namespace RazorHtmxWorkshop.Data;

public static class InMemoryTaskStore
{
    private static int _nextId = 1;
    private static readonly List<TaskItem> _tasks = new();

    public static IReadOnlyList<TaskItem> All() => _tasks
        .OrderByDescending(t => t.CreatedUtc)
        .ToList();

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

    public static bool Delete(int id)
    {
        var task = _tasks.FirstOrDefault(t => t.Id == id);
        if (task is null) return false;

        _tasks.Remove(task);
        return true;
    }
}
```

---

## Verification Checklist

Before moving to Lab 5, verify these behaviors:

### Details Pattern

- [ ] Clicking "Details" loads task info into panel/modal
- [ ] Details content updates without page reload
- [ ] Not-found case shows appropriate message

### Delete Pattern

- [ ] Clicking "Delete" shows browser confirmation dialog
- [ ] Confirming deletes task and updates list
- [ ] Canceling keeps task in list
- [ ] Success message appears after delete

### Filtering

- [ ] Typing in filter updates list after 400ms
- [ ] Filter is applied (shows matching tasks only)
- [ ] URL updates with query parameter (`?handler=List&q=...`)
- [ ] "Clear filter" link removes filter

### Pagination

- [ ] Pagination controls appear when tasks > page size
- [ ] Clicking page number updates list
- [ ] URL updates with page parameter
- [ ] Browser back/forward navigates through history

### Transitions

- [ ] List fades during swap
- [ ] Loading indicator appears during requests
- [ ] Buttons are disabled during their request

---

## Key Takeaways

### Patterns Summary

| Pattern         | Key Attributes                     | Purpose                       |
|-----------------|------------------------------------|-------------------------------|
| **Details**     | `hx-get`, `hx-target`              | Load content into panel/modal |
| **Delete**      | `hx-post`, `hx-confirm`, `hx-vals` | Safe destructive actions      |
| **Filter**      | `hx-trigger`, `hx-push-url`        | Live search with URL state    |
| **Pagination**  | `hx-push-url` on links             | Navigable, bookmarkable pages |
| **Transitions** | CSS + htmx classes                 | Visual polish                 |

### URL State Philosophy

The key insight of this lab is that **URLs should reflect application state**:

- Filter query → `?q=buy`
- Current page → `?page=2`
- Page size → `?pageSize=10`

This enables:

- **Bookmarking**: Users save filtered/paginated views
- **Sharing**: Send a link to a specific view
- **History**: Back/forward buttons work correctly
- **SEO**: Search engines can index different views

### Swap Strategy Selection

| If your response...               | Use swap strategy...        |
|-----------------------------------|-----------------------------|
| Includes wrapper element with ID  | `outerHTML`                 |
| Is content for a container        | `innerHTML`                 |
| Is a new item to add to a list    | `beforeend` or `afterbegin` |
| Triggers events only (no content) | `none`                      |

---

## Troubleshooting

### Common Issues

| Problem                               | Likely Cause                       | Solution                          |
|---------------------------------------|------------------------------------|-----------------------------------|
| Modal doesn't open                    | Missing Bootstrap JS               | Ensure Bootstrap JS is loaded     |
| Modal opens but content is stale      | Wrong swap strategy                | Use `innerHTML` for modal body    |
| URL doesn't update                    | Missing `hx-push-url`              | Add `hx-push-url="true"`          |
| Back button doesn't work              | Initial page doesn't handle params | Ensure `OnGet` reads query params |
| Filter doesn't preserve on pagination | Missing `q=@Model.Query`           | Include query in pagination links |
| Delete fails silently                 | Missing `hx-confirm` handling      | Check browser console for errors  |

### Debug Tips

1. **Check Network tab**: Verify request URL includes all parameters
2. **Check Response**: Confirm fragment structure matches swap strategy
3. **Check URL bar**: Verify `hx-push-url` is updating the URL
4. **Check Console**: Look for htmx errors or warnings
5. **Test back/forward**: Ensure page loads with correct state

---

## What Comes Next

In **Lab 5**, you'll implement:

- Dynamic form rows (Add/Remove tag inputs)
- Dependent dropdowns (Category → Subcategory)
- Long-running operations with polling
- Out-of-band swaps for global updates

**Proceed to Lab 5: Dynamic Forms + Long-Running UX (Polling) →**
