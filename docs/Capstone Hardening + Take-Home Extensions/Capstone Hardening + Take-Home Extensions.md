---
order: 1
icon: check-circle-fill
---

# Capstone: Hardening + Take-Home Extensions

## Overview

This capstone session transforms your workshop labs into a **repeatable blueprint** you can apply to real projects. We'll consolidate what you've learned, establish conventions you can document for your team, and explore advanced extensions you can implement on your own.

### Session Goals

| Goal            | Description                                                 |
|-----------------|-------------------------------------------------------------|
| **Consolidate** | Clean up code, remove duplication, standardize patterns     |
| **Document**    | Create a convention checklist for your team                 |
| **Extend**      | Explore advanced patterns (inline edit, optimistic UI, SSE) |
| **Adopt**       | Understand how to introduce htmx to existing applications   |

### Time Estimate

15–20 minutes (core) + take-home extensions

---

## Part 1: Clean-Up Pass (5–7 minutes)

Before leaving the workshop, let's ensure your codebase is well-organized and ready to serve as a reference.

### 1.1 Fragment Consolidation

Review your `Pages/Tasks/Partials/` folder and ensure each fragment has a single responsibility:

**Current Fragment Inventory:**

| Fragment                    | Purpose                      | Model                              |
|-----------------------------|------------------------------|------------------------------------|
| `_TaskList.cshtml`          | List with pagination         | `TaskListVm`                       |
| `_TaskForm.cshtml`          | Create/edit form             | `IndexModel`                       |
| `_Messages.cshtml`          | Success/error messages       | `string?`                          |
| `_TaskDetails.cshtml`       | Detail view (panel/modal)    | `TaskItem?`                        |
| `_TitleValidation.cshtml`   | Field-level validation       | `string?`                          |
| `_TagRow.cshtml`            | Single tag input             | `(int, string)`                    |
| `_TagsContainer.cshtml`     | Tags section with add button | `List<string>`                     |
| `_SubcategorySelect.cshtml` | Dependent dropdown           | `(IReadOnlyList<string>, string?)` |
| `_JobStatus.cshtml`         | Polling job status           | `JobStatus?`                       |
| `_JobStatusWithOob.cshtml`  | Job status + OOB message     | `(JobStatus, string, string)`      |
| `_Error.cshtml`             | Generic error display        | `string`                           |

### 1.2 Remove Duplication

**Check for repeated patterns:**

```csharp
// BEFORE: Repeated htmx detection
if (Request.Headers.TryGetValue("HX-Request", out var v) && v == "true")
{
    // ...
}

// AFTER: Use helper method consistently
if (IsHtmx())
{
    // ...
}
```

**Check for repeated fragment returns:**

```csharp
// BEFORE: Inline PartialViewResult construction
return new PartialViewResult
{
    ViewName = "Partials/_TaskList",
    ViewData = new ViewDataDictionary(ViewData) { Model = tasks }
};

// AFTER: Use helper method
return Fragment("Partials/_TaskList", tasks);
```

### 1.3 Tighten Target IDs

Ensure every swappable region has:

1. **A stable, unique ID** in the fragment
2. **Consistent naming** (`#task-list`, not `#taskList` or `#list`)
3. **Documentation** (comment in the partial)

**Example: Add ID documentation to each fragment:**

```cshtml
@*
    Fragment: _TaskList
    Target ID: #task-list
    Swap: outerHTML
    Returned by: OnGetList, OnPostCreate, OnPostDelete
*@
<div id="task-list">
    @* content *@
</div>
```

### 1.4 Handler Organization

Group handlers logically in your PageModel:

```csharp
public class IndexModel : PageModel
{
    // ═══════════════════════════════════════════════════════════
    // Properties
    // ═══════════════════════════════════════════════════════════

    public IReadOnlyList<TaskItem> Tasks { get; private set; } = Array.Empty<TaskItem>();

    [BindProperty]
    public NewTaskInput Input { get; set; } = new();

    [TempData]
    public string? FlashMessage { get; set; }

    // ═══════════════════════════════════════════════════════════
    // Page Lifecycle
    // ═══════════════════════════════════════════════════════════

    public void OnGet(string? q, int page = 1, int pageSize = 5) { }

    // ═══════════════════════════════════════════════════════════
    // List Fragment Handlers
    // ═══════════════════════════════════════════════════════════

    public IActionResult OnGetList(string? q, int page = 1, int pageSize = 5) { }
    public IActionResult OnGetDetails(int id) { }

    // ═══════════════════════════════════════════════════════════
    // Form Fragment Handlers
    // ═══════════════════════════════════════════════════════════

    public IActionResult OnGetEmptyForm() { }
    public IActionResult OnPostCreate() { }
    public IActionResult OnPostValidateTitle() { }

    // ═══════════════════════════════════════════════════════════
    // Dynamic Form Handlers
    // ═══════════════════════════════════════════════════════════

    public IActionResult OnGetAddTag() { }
    public IActionResult OnGetRemoveTag() { }
    public IActionResult OnGetSubcategories([FromQuery(Name = "Input.Category")] string? category) { }

    // ═══════════════════════════════════════════════════════════
    // CRUD Handlers
    // ═══════════════════════════════════════════════════════════

    public IActionResult OnPostDelete(int id) { }

    // ═══════════════════════════════════════════════════════════
    // Background Job Handlers
    // ═══════════════════════════════════════════════════════════

    public IActionResult OnPostStartJob() { }
    public IActionResult OnGetJobStatus(string jobId) { }
    public IActionResult OnGetResetJob() { }

    // ═══════════════════════════════════════════════════════════
    // Message Handlers
    // ═══════════════════════════════════════════════════════════

    public IActionResult OnGetMessages() { }

    // ═══════════════════════════════════════════════════════════
    // Helper Methods
    // ═══════════════════════════════════════════════════════════

    private bool IsHtmx() =>
        Request.Headers.TryGetValue("HX-Request", out var v) && v == "true";

    private PartialViewResult Fragment(string partialName, object model) =>
        new()
        {
            ViewName = partialName,
            ViewData = new ViewDataDictionary(MetadataProvider, ModelState) { Model = model }
        };
}
```

### 1.5 Clean-Up Checklist

Before moving on, verify:

- [ ] All fragments have consistent ID documentation
- [ ] All handlers use `IsHtmx()` helper
- [ ] All fragment returns use `Fragment()` helper
- [ ] Handlers are grouped logically
- [ ] No duplicate code patterns exist
- [ ] Unused code/comments are removed

---

## Part 2: Convention Checklist (5–7 minutes)

Create a reference document your team can use when building htmx + Razor Pages features.

### 2.1 The Complete Convention Checklist

Save this as `HTMX_CONVENTIONS.md` in your project root:

```markdown
# htmx + Razor Pages Conventions

## Fragment Rules

### Naming
- Partial files: `_PascalCase.cshtml` (underscore prefix)
- Fragment IDs: `#kebab-case` (lowercase, hyphens)
- Handlers: `OnGet{Resource}` / `OnPost{Action}`

### Structure
- Every fragment MUST have a stable wrapper element with an ID
- The wrapper ID MUST match the hx-target selector
- Fragments are self-contained (no dependencies on page layout)

### Example
```cshtml
@* Fragment: _TaskList *@
@* Target: #task-list *@
@* Swap: outerHTML *@
<div id="task-list">
    <!-- content -->
</div>
```

## Response Rules

### When to return what

| Scenario              | Response                                |
|-----------------------|-----------------------------------------|
| Initial page load     | `Page()`                                |
| Non-htmx form submit  | `RedirectToPage()`                      |
| htmx success          | `Fragment("...", model)`                |
| htmx validation error | `Fragment("_Form", this)` + retarget    |
| htmx not found        | `Fragment("_Messages", msg)` + retarget |
| htmx server error     | `Fragment("_Error", msg)` + retarget    |

### Status Codes
- 200: Success
- 302: Redirect (non-htmx)

## Swap Strategies

| Strategy     | Use When                                |
|--------------|-----------------------------------------|
| `outerHTML`  | Fragment includes its wrapper (default) |
| `innerHTML`  | Swapping content inside a container     |
| `beforeend`  | Appending to a list                     |
| `afterbegin` | Prepending to a list                    |
| `delete`     | Removing an element                     |
| `none`       | Side effects only (triggers)            |

## htmx Attributes Quick Reference

### Requests
- `hx-get="?handler=List"` - GET request
- `hx-post="?handler=Create"` - POST request
- `hx-vals='{"key": value}'` - Include additional values

### Targeting
- `hx-target="#element-id"` - Where to swap
- `hx-swap="outerHTML"` - How to swap

### Triggers
- `hx-trigger="click"` - On click (default for buttons)
- `hx-trigger="change"` - On change (default for select)
- `hx-trigger="keyup changed delay:500ms"` - Debounced input
- `hx-trigger="every 1s"` - Polling

### Extras
- `hx-confirm="Are you sure?"` - Confirmation dialog
- `hx-indicator="#loading"` - Show during request
- `hx-include="closest form"` - Include form data
- `hx-push-url="true"` - Update browser URL

## Response Headers

### From Server
- `HX-Trigger: eventName` - Fire client event
- `HX-Retarget: #selector` - Override hx-target
- `HX-Reswap: innerHTML` - Override hx-swap
- `HX-Push-Url: /path` - Update browser URL

## Validation Pattern

### Field-level (keyup)
```html
<input hx-post="?handler=ValidateField"
       hx-trigger="keyup changed delay:500ms"
       hx-target="#field-validation"
       hx-swap="outerHTML"
       hx-include="closest form" />
```

### Form-level (submit)
```html
<form hx-post="?handler=Create"
      hx-target="#list"
      hx-swap="outerHTML">
```

On validation failure: retarget to form.

## Error Handling Pattern

```csharp
if (IsHtmx())
{
    Response.Headers["HX-Retarget"] = "#messages";
    Response.Headers["HX-Reswap"] = "outerHTML";
    return Fragment("Partials/_Messages", errorMessage);
}
```

## Polling Pattern

```cshtml
@if (Model.State == "running")
{
    <div hx-get="?handler=Status&id=@Model.Id"
         hx-trigger="every 1s"
         hx-target="#status"
         hx-swap="outerHTML">
        <!-- progress content -->
    </div>
}
else
{
    <div id="status">
        <!-- final content - no polling -->
    </div>
}
```

## OOB Swap Pattern

```cshtml
<!-- Primary fragment -->
<div id="primary-target">
    <!-- main content -->
</div>

<!-- Out-of-band fragment -->
<div id="secondary-target" hx-swap-oob="true">
    <!-- additional content -->
</div>
```
```

### 2.2 Quick Reference Card

Create a printable reference card:

```markdown
# htmx + Razor Pages Quick Reference

## Request → Handler → Response

```
hx-get="?handler=List"    → OnGetList()    → Fragment("_List", data)
hx-post="?handler=Create" → OnPostCreate() → Fragment("_List", data)
```

## The Three Questions

1. **What triggers the request?** (hx-trigger)
2. **Where does the response go?** (hx-target + hx-swap)
3. **What does the server return?** (Fragment or Page)

## Common Patterns

| Pattern | Trigger | Target | Handler Returns |
|---------|---------|--------|-----------------|
| Refresh list | click | #list | _List |
| Submit form | submit | #list | _List (or _Form on error) |
| Validate field | keyup delay:500ms | #field-error | _FieldValidation |
| Load details | click | #details | _Details |
| Delete item | click + confirm | #list | _List |
| Poll status | every 1s | #status | _Status |
| Add row | click | #container beforeend | _Row |
| Cascade dropdown | change | #child-select | _ChildOptions |

## Debugging Checklist

1. Network tab: Is request firing?
2. Request URL: Does handler match?
3. Response body: Is it HTML fragment?
4. Target exists: Is ID in DOM?
5. Swap strategy: Matches fragment structure?
```

---

## Part 3: Optional Extensions (Take-Home)

These extensions build on the patterns you've learned. Implement them to deepen your understanding.

### Extension 1: Inline Edit Row (Intermediate)

**Goal:** Click an item to edit it inline, save without leaving the list.

**Design:**

1. Each list item has an "Edit" button
2. Clicking Edit replaces the row with an edit form
3. Save replaces the form with the updated row
4. Cancel restores the original row

**Implementation:**

**File: `Pages/Tasks/Partials/_TaskRow.cshtml`**

```cshtml
@model RazorHtmxWorkshop.Models.TaskItem

@*
    Task Row Fragment (View Mode)
    =============================

    Purpose: Display a single task in the list
    Swap target: #task-row-{id}

    Design: Clicking Edit fetches the edit form for this row
*@

<li class="list-group-item" id="task-row-@Model.Id">
    <div class="d-flex justify-content-between align-items-center">
        <div>
            <strong>@Model.Title</strong>
            <span class="text-muted small ms-2">
                Created @Model.CreatedUtc.ToLocalTime().ToString("g")
            </span>
        </div>
        <div class="btn-group btn-group-sm">
            <button type="button"
                    class="btn btn-outline-secondary"
                    hx-get="?handler=EditRow&id=@Model.Id"
                    hx-target="#task-row-@Model.Id"
                    hx-swap="outerHTML">
                Edit
            </button>
            <button type="button"
                    class="btn btn-outline-danger"
                    hx-post="?handler=Delete"
                    hx-vals='{"id": @Model.Id}'
                    hx-confirm="Delete this task?"
                    hx-target="#task-list"
                    hx-swap="outerHTML">
                Delete
            </button>
        </div>
    </div>
</li>
```

**File: `Pages/Tasks/Partials/_TaskRowEdit.cshtml`**

```cshtml
@model RazorHtmxWorkshop.Models.TaskItem

@*
    Task Row Fragment (Edit Mode)
    =============================

    Purpose: Inline edit form for a single task
    Swap target: #task-row-{id}

    Design: Save updates the task and returns view mode
            Cancel returns view mode without changes
*@

<li class="list-group-item" id="task-row-@Model.Id">
    <form hx-post="?handler=UpdateRow"
          hx-target="#task-row-@Model.Id"
          hx-swap="outerHTML"
          class="d-flex gap-2 align-items-center">

        @Html.AntiForgeryToken()
        <input type="hidden" name="id" value="@Model.Id" />

        <input type="text"
               name="title"
               value="@Model.Title"
               class="form-control form-control-sm"
               required
               autofocus />

        <div class="btn-group btn-group-sm">
            <button type="submit" class="btn btn-primary">
                Save
            </button>
            <button type="button"
                    class="btn btn-outline-secondary"
                    hx-get="?handler=CancelEdit&id=@Model.Id"
                    hx-target="#task-row-@Model.Id"
                    hx-swap="outerHTML">
                Cancel
            </button>
        </div>
    </form>
</li>
```

**Handlers:**

```csharp
/// <summary>
/// Returns the edit form for a specific row.
/// </summary>
public IActionResult OnGetEditRow(int id)
{
    var task = InMemoryTaskStore.Find(id);
    if (task is null)
    {
        return Content("<li class='list-group-item text-danger'>Task not found</li>", "text/html");
    }

    return Fragment("Partials/_TaskRowEdit", task);
}

/// <summary>
/// Updates a task and returns the view mode row.
/// </summary>
public IActionResult OnPostUpdateRow(int id, string title)
{
    var task = InMemoryTaskStore.Find(id);
    if (task is null)
    {
        return Content("<li class='list-group-item text-danger'>Task not found</li>", "text/html");
    }

    if (string.IsNullOrWhiteSpace(title) || title.Length < 3)
    {
        // Return edit form with error
        return Fragment("Partials/_TaskRowEdit", task);
    }

    // Update the task
    InMemoryTaskStore.Update(id, title.Trim());
    task = InMemoryTaskStore.Find(id)!;

    return Fragment("Partials/_TaskRow", task);
}

/// <summary>
/// Cancels editing and returns the view mode row.
/// </summary>
public IActionResult OnGetCancelEdit(int id)
{
    var task = InMemoryTaskStore.Find(id);
    if (task is null)
    {
        return Content("<li class='list-group-item text-danger'>Task not found</li>", "text/html");
    }

    return Fragment("Partials/_TaskRow", task);
}
```

**Add to InMemoryTaskStore:**

```csharp
public static void Update(int id, string newTitle)
{
    var task = _tasks.FirstOrDefault(t => t.Id == id);
    if (task is not null)
    {
        task.Title = newTitle;
    }
}
```

---

### Extension 2: Optimistic UI with Disabled Buttons (Beginner)

**Goal:** Disable buttons and show loading state during requests to prevent double-submits.

**Design:**

1. Button shows spinner and disables during request
2. Uses htmx CSS classes (`htmx-request`)
3. No JavaScript required

**Implementation:**

**CSS (add to site.css):**

```css
/* ═══════════════════════════════════════════════════════════════
   Optimistic UI - Button States
   ═══════════════════════════════════════════════════════════════ */

/* Disable buttons during htmx requests */
button.htmx-request,
.htmx-request button {
    pointer-events: none;
    opacity: 0.65;
    cursor: not-allowed;
}

/* Hide button text during request */
button.htmx-request .btn-text {
    visibility: hidden;
}

/* Show spinner during request */
button .btn-spinner {
    display: none;
}

button.htmx-request .btn-spinner {
    display: inline-block;
    position: absolute;
    left: 50%;
    transform: translateX(-50%);
}

/* Button needs relative positioning for spinner */
button.btn-loading {
    position: relative;
}

/* ═══════════════════════════════════════════════════════════════
   Optimistic UI - Form States
   ═══════════════════════════════════════════════════════════════ */

/* Dim form during submission */
form.htmx-request {
    opacity: 0.7;
    pointer-events: none;
}

/* ═══════════════════════════════════════════════════════════════
   Optimistic UI - List Item States
   ═══════════════════════════════════════════════════════════════ */

/* Highlight newly added items */
.list-group-item.htmx-added {
    animation: highlight-new 1s ease-out;
}

@keyframes highlight-new {
    from {
        background-color: rgba(25, 135, 84, 0.2);
    }
    to {
        background-color: transparent;
    }
}

/* Fade items being deleted */
.list-group-item.htmx-swapping {
    opacity: 0;
    transition: opacity 200ms ease-out;
}
```

**Button with Loading State:**

```cshtml
<button type="submit" class="btn btn-primary btn-loading">
    <span class="btn-text">Add Task</span>
    <span class="btn-spinner spinner-border spinner-border-sm" role="status">
        <span class="visually-hidden">Loading...</span>
    </span>
</button>
```

**Delete Button with Loading:**

```cshtml
<button type="button"
        class="btn btn-sm btn-outline-danger btn-loading"
        hx-post="?handler=Delete"
        hx-vals='{"id": @task.Id}'
        hx-confirm="Delete this task?"
        hx-target="#task-list"
        hx-swap="outerHTML">
    <span class="btn-text">Delete</span>
    <span class="btn-spinner spinner-border spinner-border-sm" role="status"></span>
</button>
```

---

### Extension 3: SSE (Server-Sent Events) for Live Updates (Advanced)

**Goal:** Push updates to the browser without polling.

**Design:**

1. Server sends events when data changes
2. htmx SSE extension listens for events
3. Events trigger fragment refreshes

**Implementation:**

**1. Add htmx SSE Extension:**

```html
<!-- In _Layout.cshtml, after htmx script -->
<script src="https://unpkg.com/htmx-ext-sse@2.2.1/sse.js"></script>
```

**2. Create SSE Endpoint:**

**File: `Pages/Tasks/TaskEvents.cshtml.cs`**

```csharp
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace RazorHtmxWorkshop.Pages.Tasks;

public class TaskEventsModel : PageModel
{
    /// <summary>
    /// SSE endpoint for task updates.
    /// Sends events when tasks change.
    /// </summary>
    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        Response.ContentType = "text/event-stream";
        Response.Headers["Cache-Control"] = "no-cache";
        Response.Headers["Connection"] = "keep-alive";

        // In production: subscribe to a message bus or change feed
        // For demo: send heartbeat every 5 seconds

        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                // Check for changes (in production: use proper event source)
                var hasChanges = TaskChangeTracker.HasChanges();

                if (hasChanges)
                {
                    // Send event that triggers list refresh
                    await Response.WriteAsync($"event: taskListChanged\n");
                    await Response.WriteAsync($"data: refresh\n\n");
                    await Response.Body.FlushAsync(cancellationToken);

                    TaskChangeTracker.ClearChanges();
                }

                // Heartbeat to keep connection alive
                await Response.WriteAsync($": heartbeat\n\n");
                await Response.Body.FlushAsync(cancellationToken);

                await Task.Delay(5000, cancellationToken);
            }
        }
        catch (OperationCanceledException)
        {
            // Client disconnected
        }
    }
}

/// <summary>
/// Simple change tracker for demo purposes.
/// In production, use a proper event bus.
/// </summary>
public static class TaskChangeTracker
{
    private static bool _hasChanges = false;
    private static readonly object _lock = new();

    public static void NotifyChange()
    {
        lock (_lock) { _hasChanges = true; }
    }

    public static bool HasChanges()
    {
        lock (_lock) { return _hasChanges; }
    }

    public static void ClearChanges()
    {
        lock (_lock) { _hasChanges = false; }
    }
}
```

**3. Connect UI to SSE:**

```cshtml
<!-- In Index.cshtml, wrap the list in SSE container -->
<div hx-ext="sse" sse-connect="/Tasks/TaskEvents">
    <div id="task-list"
         sse-swap="taskListChanged"
         hx-get="?handler=List"
         hx-trigger="sse:taskListChanged"
         hx-target="#task-list"
         hx-swap="outerHTML">
        @* list content *@
    </div>
</div>
```

**4. Trigger Changes:**

```csharp
// In OnPostCreate, OnPostDelete, etc.
public IActionResult OnPostCreate()
{
    // ... existing code ...

    // Notify SSE listeners
    TaskChangeTracker.NotifyChange();

    // ... return fragment ...
}
```

---

### Extension 4: Authorization Edge Cases (Intermediate)

**Goal:** Handle authorization properly with htmx requests.

**Design:**

1. Return appropriate status codes for auth failures
2. Redirect to login for unauthenticated users
3. Show permission errors for unauthorized actions

**Implementation:**

**Custom Authorization Filter:**

```csharp
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

public class HtmxAuthorizeAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var user = context.HttpContext.User;

        if (!user.Identity?.IsAuthenticated ?? true)
        {
            var isHtmx = context.HttpContext.Request.Headers
                .TryGetValue("HX-Request", out var v) && v == "true";

            if (isHtmx)
            {
                // For htmx: return 401 with redirect header
                context.HttpContext.Response.StatusCode = 401;
                context.HttpContext.Response.Headers["HX-Redirect"] = "/Identity/Account/Login";
                context.Result = new EmptyResult();
            }
            else
            {
                // For regular requests: redirect normally
                context.Result = new RedirectToPageResult("/Identity/Account/Login");
            }

            return;
        }

        base.OnActionExecuting(context);
    }
}
```

**Permission Check Helper:**

```csharp
private IActionResult? CheckPermission(int taskId, string requiredPermission)
{
    // Example: Check if user can modify this task
    var canModify = /* your permission logic */;

    if (!canModify)
    {
        if (IsHtmx())
        {
            Response.StatusCode = 403;
            Response.Headers["HX-Retarget"] = "#messages";
            Response.Headers["HX-Reswap"] = "outerHTML";
            return Fragment("Partials/_Messages", "You don't have permission to modify this task.");
        }

        return Forbid();
    }

    return null; // Permission granted
}

// Usage in handler:
public IActionResult OnPostDelete(int id)
{
    var permissionResult = CheckPermission(id, "delete");
    if (permissionResult is not null) return permissionResult;

    // ... proceed with delete ...
}
```

**Handle Expired Sessions:**

```javascript
// In site.js or inline script
document.body.addEventListener('htmx:responseError', function(event) {
    if (event.detail.xhr.status === 401) {
        // Session expired - redirect to login
        window.location.href = '/Identity/Account/Login';
    }
});
```

---

### Extension 5: Keyboard Shortcuts (Beginner)

**Goal:** Add keyboard navigation for power users.

**Implementation:**

```javascript
// In site.js
document.addEventListener('DOMContentLoaded', function() {
    // Global shortcuts
    document.addEventListener('keydown', function(e) {
        // Ctrl+N: Focus new task input
        if (e.ctrlKey && e.key === 'n') {
            e.preventDefault();
            document.querySelector('#title')?.focus();
        }

        // Escape: Clear form
        if (e.key === 'Escape') {
            const form = document.querySelector('#task-form form');
            if (form) {
                form.reset();
                document.querySelector('#title')?.blur();
            }
        }

        // Ctrl+Enter: Submit form
        if (e.ctrlKey && e.key === 'Enter') {
            const form = document.querySelector('#task-form form');
            if (form && document.activeElement?.closest('#task-form')) {
                htmx.trigger(form, 'submit');
            }
        }
    });
});
```

**Add to UI:**

```cshtml
<div class="small text-muted mt-2">
    <kbd>Ctrl</kbd>+<kbd>N</kbd> New task |
    <kbd>Ctrl</kbd>+<kbd>Enter</kbd> Submit |
    <kbd>Esc</kbd> Clear
</div>
```

---

## Part 4: Final Recap and Adoption Path

### 4.1 The Mental Model

**Core Concept: Server-Driven UI**

```
┌─────────────────────────────────────────────────────────────┐
│                         Browser                              │
│  ┌─────────┐    htmx     ┌─────────┐    htmx     ┌───────┐ │
│  │ Trigger │ ──────────► │ Request │ ◄────────── │ Swap  │ │
│  │ (click) │             │ (POST)  │             │ (DOM) │ │
│  └─────────┘             └────┬────┘             └───┬───┘ │
│                               │                       ▲     │
└───────────────────────────────┼───────────────────────┼─────┘
                                │                       │
                                ▼                       │
┌───────────────────────────────┼───────────────────────┼─────┐
│                               │        Server         │     │
│  ┌─────────┐    Razor    ┌────┴────┐    Razor    ┌───┴───┐ │
│  │ Handler │ ──────────► │ Partial │ ◄────────── │ Model │ │
│  │ (C#)    │             │ (HTML)  │             │ (C#)  │ │
│  └─────────┘             └─────────┘             └───────┘ │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

**The Flow:**

1. **User action** triggers htmx request
2. **Server handler** processes request
3. **Razor partial** renders HTML fragment
4. **htmx** swaps fragment into DOM
5. **Browser** displays updated UI

**What Makes This Powerful:**

- Server controls all logic and state
- HTML is the API contract
- No JSON parsing or DOM manipulation
- Progressive enhancement built-in
- Works with any backend

### 4.2 Adoption Path for Real Applications

**Phase 1: Start Small (Week 1)**

```
Choose ONE feature to enhance:
├── A simple list with add/remove
├── A form with inline validation
├── A details panel or modal
└── A filter/search component

Goal: Learn the pattern in isolation
```

**Phase 2: Establish Conventions (Week 2)**

```
Document your patterns:
├── Create fragment naming conventions
├── Define response rules
├── Set up helper methods
└── Build a style guide

Goal: Consistency across features
```

**Phase 3: Expand Gradually (Weeks 3-4)**

```
Apply to more features:
├── Convert existing AJAX to htmx
├── Add real-time validation
├── Implement polling where needed
└── Add OOB swaps for notifications

Goal: htmx becomes the default approach
```

**Phase 4: Advanced Patterns (Ongoing)**

```
Optimize and enhance:
├── SSE for live updates
├── Optimistic UI
├── Offline support
└── Performance tuning

Goal: Polish and production-ready
```

### 4.3 Common Adoption Questions

**Q: Can htmx work alongside existing JavaScript?**

A: Yes. htmx is just a library that processes HTML attributes. Your existing JavaScript continues to work. You can mix approaches—use htmx for server-rendered fragments and JavaScript for purely client-side interactions.

**Q: What about forms that need client-side validation?**

A: Use both! Client-side validation for immediate feedback, server-side for authority. htmx doesn't prevent you from using `required`, `pattern`, or JavaScript validation.

**Q: How do I handle offline scenarios?**

A: htmx works best with connectivity. For offline support, consider:
- Service workers for caching
- Local storage for queuing actions
- Progressive enhancement (forms work without JS)

**Q: What about SEO?**

A: Server-rendered HTML is SEO-friendly by default. Use `hx-push-url` to update URLs for shareable/indexable state. Initial page loads render full content.

**Q: How do I test htmx interactions?**

A: Test at two levels:
1. **Unit tests**: Test handlers return correct fragments
2. **Integration tests**: Use browser automation (Playwright, Selenium) to verify swaps

### 4.4 Resources for Continued Learning

**Official Resources:**

- htmx.org - Documentation and examples
- htmx.org/essays - Philosophy and patterns
- GitHub: bigskysoftware/htmx - Source and issues

**Community:**

- htmx Discord server
- r/htmx on Reddit
- #htmx on Twitter/X

**Books and Courses:**

- "Hypermedia Systems" by Carson Gross (htmx creator)
- Various YouTube tutorials and conference talks

**ASP.NET Core Specific:**

- Microsoft Docs: Razor Pages
- htmx + ASP.NET Core blog posts
- This workshop materials!

---

## Verification Checklist (Final)

Before leaving the workshop, confirm:

### Code Quality

- [ ] All fragments have documented IDs
- [ ] All handlers use consistent helpers
- [ ] No duplicate patterns exist
- [ ] Code is organized logically

### Documentation

- [ ] Convention checklist is saved
- [ ] Quick reference card is printed/saved
- [ ] Fragment inventory is complete

### Understanding

- [ ] I can explain the htmx mental model
- [ ] I know when to use each swap strategy
- [ ] I understand the response rules
- [ ] I can debug htmx interactions

### Next Steps

- [ ] I've identified a feature to convert
- [ ] I have resources bookmarked
- [ ] I know where to get help

---

## Congratulations!

You've completed the htmx + ASP.NET Core Workshop!

### What You've Learned

| Lab          | Key Skills                                            |
|--------------|-------------------------------------------------------|
| **Lab 1**    | Fragment boundaries, partial views, stable IDs        |
| **Lab 2**    | `hx-get`, `hx-post`, targeting, swapping, retargeting |
| **Lab 3**    | Real-time validation, debouncing, antiforgery         |
| **Lab 4**    | Modals, confirm dialogs, URL state, pagination        |
| **Lab 5**    | Dynamic forms, dependent selects, polling, OOB swaps  |
| **Capstone** | Conventions, hardening, advanced extensions           |

### The htmx Philosophy

> "HTML is the contract. The server is in control. Complexity is optional."

Take this approach back to your projects. Start small, establish conventions, and expand gradually.

### Stay Connected

Share your htmx projects and questions:
- Workshop feedback: [your contact]
- htmx community: discord.gg/htmx
- Twitter/X: #htmx

**Thank you for attending!**

---

## Appendix: Complete Handler Reference

For reference, here's a complete handler inventory from all labs:

| Handler               | Verb | Fragment             | Purpose                 |
|-----------------------|------|----------------------|-------------------------|
| `OnGet`               | GET  | (Page)               | Initial page load       |
| `OnGetList`           | GET  | `_TaskList`          | Fetch/refresh list      |
| `OnGetDetails`        | GET  | `_TaskDetails`       | Load item details       |
| `OnGetMessages`       | GET  | `_Messages`          | Fetch messages          |
| `OnGetEmptyForm`      | GET  | `_TaskForm`          | Reset form              |
| `OnGetAddTag`         | GET  | `_TagRow`            | Add tag input           |
| `OnGetRemoveTag`      | GET  | (empty)              | Remove tag              |
| `OnGetSubcategories`  | GET  | `_SubcategorySelect` | Update dropdown         |
| `OnGetJobStatus`      | GET  | `_JobStatus`         | Poll job progress       |
| `OnGetResetJob`       | GET  | `_JobStatus`         | Reset job UI            |
| `OnGetEditRow`        | GET  | `_TaskRowEdit`       | Edit mode (extension)   |
| `OnGetCancelEdit`     | GET  | `_TaskRow`           | Cancel edit (extension) |
| `OnPostCreate`        | POST | `_TaskList`          | Create item             |
| `OnPostDelete`        | POST | `_TaskList`          | Delete item             |
| `OnPostReset`         | POST | (Page)               | Reset all tasks         |
| `OnPostValidateTitle` | POST | `_TitleValidation`   | Validate field          |
| `OnPostStartJob`      | POST | `_JobStatus`         | Start background job    |
| `OnPostUpdateRow`     | POST | `_TaskRow`           | Update item (extension) |
