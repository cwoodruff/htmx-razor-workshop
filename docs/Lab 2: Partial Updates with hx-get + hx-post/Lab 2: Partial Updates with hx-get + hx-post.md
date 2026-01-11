---
order: 1
icon: code-square
---

# Lab 2: Partial Updates with hx-get + hx-post

## Overview

Welcome to Lab 2! In this lab, you will transform your traditional server-rendered application into an interactive experience using htmx. You'll learn the primary htmx workflow: **request a server-rendered fragment and swap it into a target**.

By the end of this lab, your Tasks page will:

- Submit forms without full page reloads
- Update only the parts of the page that changed
- Show loading indicators during requests
- Handle validation errors gracefully

### The Core htmx Philosophy

htmx follows a simple but powerful pattern:

1. **Trigger**: User action (click, submit, keyup, etc.)
2. **Request**: htmx sends an AJAX request to the server
3. **Response**: Server returns an HTML fragment (not JSON, not a full page)
4. **Swap**: htmx replaces a target element with the returned HTML

This is "HTML over the wire"—the server remains the source of truth for your UI, and htmx handles the transport and DOM manipulation.

### Why This Approach?

| Traditional SPA        | htmx Approach           |
|------------------------|-------------------------|
| Server returns JSON    | Server returns HTML     |
| Client renders UI      | Server renders UI       |
| Complex client state   | Simple request/response |
| Heavy JavaScript       | Minimal JavaScript      |
| Separate API contracts | HTML is the contract    |

---

## Lab Outcomes

By the end of Lab 2, you will be able to:

| Outcome                     | Description                                       |
|-----------------------------|---------------------------------------------------|
| **Submit via hx-post**      | Create form submits without page reload           |
| **Target specific regions** | Update only `#task-list` using `hx-target`        |
| **Swap strategies**         | Use `hx-swap="outerHTML"` to replace fragments    |
| **Refresh via hx-get**      | Add a button that fetches fresh data              |
| **Pass parameters**         | Use `hx-vals` to send additional data             |
| **Loading indicators**      | Show feedback during requests with `hx-indicator` |
| **Error handling**          | Use response headers to retarget error responses  |

---

## Prerequisites

Before starting this lab, ensure you have:

- **Completed Lab 1** with all verifications passing
- **htmx loaded** in your `_Layout.cshtml` (we'll verify this in Step 0)
- **Three partials** with stable IDs: `#messages`, `#task-form`, `#task-list`
- **Working OnPostCreate handler** that creates tasks

---

## Step 0: Verify Prerequisites (1–2 minutes)

Before adding htmx attributes, let's confirm everything is in place.

### 0.1 Verify htmx is Loaded

Open `Pages/Shared/_Layout.cshtml` and ensure htmx is included. You should see this near the bottom of the file (before the closing `</body>` tag):

**File: `Pages/Shared/_Layout.cshtml`**

```html
<!-- htmx (Local) -->
<script src="~/lib/htmx/htmx.min.js"></script>
```

> **Note**: This workshop uses htmx from local files (located in `wwwroot/lib/htmx/`) for offline workshop environments. In production, you can use a CDN like `https://unpkg.com/htmx.org@2.0.0` or install via npm.

### 0.2 Verify Fragment Structure

Open your browser's Developer Tools and confirm these elements exist in the DOM:

```html
<div id="task-list">...</div>
<div id="task-form">...</div>
<div id="messages">...</div>
```

The `id` attributes are essential—htmx will target these elements.

### 0.3 Verify htmx is Working

Open the browser console and type:

```javascript
htmx
```

You should see the htmx object. If you see `undefined`, htmx is not loaded correctly.

---

## Step 1: Add hx-post to the Existing Form (5–7 minutes)

Our first htmx enhancement is the simplest possible change: make the create form submit via AJAX instead of a full page reload.

### 1.1 Understanding the Change

Currently, when you submit the form:

1. Browser sends a POST request
2. Server processes and redirects
3. Browser loads the entire new page

With htmx:

1. htmx intercepts the submit
2. htmx sends an AJAX POST request
3. Server returns just the updated fragment
4. htmx swaps that fragment into the target

### 1.2 Update the Form Partial

Edit `Pages/Tasks/Partials/_TaskForm.cshtml` to add htmx attributes:

**File: `Pages/Tasks/Partials/_TaskForm.cshtml`**

```cshtml
@model RazorPagesHtmxWorkshop.Pages.Tasks.IndexModel

@*
    Task Form Fragment (htmx-enhanced)
    ===================================

    Target ID: #task-form
    Swap: outerHTML
    Returned by: OnGetEmptyForm, OnPostCreate (on validation error)

    htmx attributes:
    - hx-post: Send POST request to this URL
    - hx-target: Where to swap the response (#task-list on success)
    - hx-swap: How to swap (outerHTML replaces the entire target element)
    - hx-indicator: Show loading spinner during request

    On successful submit:
    - Server returns _TaskList partial
    - htmx swaps it into #task-list
    - Server sends HX-Trigger: clearForm
    - Listener fetches empty form and swaps into #task-form

    On validation error:
    - Server returns _TaskForm partial with errors (this file)
    - Server sends HX-Retarget: #task-form
    - htmx swaps form with validation errors displayed
*@

<div id="task-form">
    <form method="post" asp-page-handler="Create"
          hx-post="?handler=Create"
          hx-target="#task-list"
          hx-swap="outerHTML"
          hx-indicator="#task-loading"
          class="vstack gap-3">
        <div>
            <label class="form-label" for="title">Task title</label>
            <input id="title"
                   class="form-control form-control-lg"
                   asp-for="Input.Title"
                   placeholder="e.g., Add htmx to Razor Pages" />
            <div class="form-text">Keep it short; we're optimizing for fast feedback loops.</div>
            <span class="text-danger" asp-validation-for="Input.Title"></span>
        </div>

        <div class="d-flex gap-2">
            <button class="btn btn-primary btn-lg" type="submit">Add task</button>
            <a class="btn btn-outline-secondary btn-lg" asp-page="/Labs">Back to labs</a>
        </div>
    </form>
</div>
```

### 1.3 Understanding the htmx Attributes

| Attribute      | Value               | Purpose                                                   |
|----------------|---------------------|-----------------------------------------------------------|
| `hx-post`      | `"?handler=Create"` | Send a POST request to this URL when form submits         |
| `hx-target`    | `"#task-list"`      | Put the response into the element with id="task-list"     |
| `hx-swap`      | `"outerHTML"`       | Replace the entire target element (not just its contents) |
| `hx-indicator` | `"#task-loading"`   | Show this element as loading indicator during request     |

**Why keep `method="post"` and `asp-page-handler`?**

These provide **progressive enhancement**. If JavaScript is disabled or htmx fails to load, the form still works as a traditional form. The htmx attributes layer behavior on top without breaking the fallback.

### 1.4 Why `hx-swap="outerHTML"`?

There are several swap strategies:

| Strategy      | Behavior                          |
|---------------|-----------------------------------|
| `innerHTML`   | Replace target's children only    |
| `outerHTML`   | Replace the entire target element |
| `beforebegin` | Insert before the target          |
| `afterend`    | Insert after the target           |
| `beforeend`   | Append inside target              |
| `afterbegin`  | Prepend inside target             |

We use `outerHTML` because our partials return the complete wrapper element:

```html
<!-- Server returns this: -->
<div id="task-list">
    <ul class="list-group">...</ul>
</div>

<!-- htmx replaces the entire #task-list element -->
```

If we used `innerHTML`, htmx would try to put `<div id="task-list">` inside the existing `<div id="task-list">`, creating nested duplicates.

---

## Step 2: Convert OnPostCreate to Return Fragment for htmx (10–12 minutes)

Now we need to update the server to return just the list fragment (instead of redirecting) when htmx makes the request.

### 2.1 The Strategy: Detect htmx Requests

htmx sends a header with every request:

```
HX-Request: true
```

We'll check for this header to decide how to respond:

- **htmx request**: Return the partial fragment
- **Normal request**: Redirect (traditional PRG pattern)

### 2.2 Add Helper Methods to the PageModel

Update `Pages/Tasks/Index.cshtml.cs` with these additions:

**File: `Pages/Tasks/Index.cshtml.cs`**

```csharp
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

    // ═══════════════════════════════════════════════════════════
    // Helper Methods
    // ═══════════════════════════════════════════════════════════

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
    /// <param name="partialName">Path to the partial view</param>
    /// <param name="model">Model to pass to the partial</param>
    private PartialViewResult Fragment(string partialName, object model) =>
        new()
        {
            ViewName = partialName,
            ViewData = new ViewDataDictionary(MetadataProvider, ModelState) { Model = model }
        };

    // ═══════════════════════════════════════════════════════════
    // Page Lifecycle
    // ═══════════════════════════════════════════════════════════

    public void OnGet()
    {
        Tasks = InMemoryTaskStore.All();
    }

    // ═══════════════════════════════════════════════════════════
    // List Fragment Handlers
    // ═══════════════════════════════════════════════════════════

    /// <summary>
    /// Handles GET requests to /Tasks?handler=List.
    /// Returns just the task list fragment for htmx to swap.
    ///
    /// Optional parameter 'take' limits the number of tasks returned.
    /// </summary>
    /// <param name="take">Optional: limit results to this many tasks</param>
    public IActionResult OnGetList(int? take)
    {
        var tasks = InMemoryTaskStore.All();

        if (take is > 0)
        {
            tasks = tasks.Take(take.Value).ToList();
        }

        return Fragment("Partials/_TaskList", tasks);
    }

    // ═══════════════════════════════════════════════════════════
    // Form Fragment Handlers
    // ═══════════════════════════════════════════════════════════

    /// <summary>
    /// Returns an empty form fragment.
    /// Called via htmx trigger after successful task creation.
    /// </summary>
    public IActionResult OnGetEmptyForm()
    {
        Input = new NewTaskInput();
        ModelState.Clear();
        return Fragment("Partials/_TaskForm", this);
    }

    // ═══════════════════════════════════════════════════════════
    // CRUD Handlers
    // ═══════════════════════════════════════════════════════════

    public IActionResult OnPostCreate()
    {
        // Validation
        if (string.IsNullOrWhiteSpace(Input.Title))
        {
            ModelState.AddModelError(nameof(Input.Title), "Title is required.");
        }

        if (!ModelState.IsValid)
        {
            Tasks = InMemoryTaskStore.All();

            if (IsHtmx())
            {
                // For htmx: return the form fragment with validation errors
                // Use response headers to retarget the swap to the form
                Response.Headers["HX-Retarget"] = "#task-form";
                Response.Headers["HX-Reswap"] = "outerHTML";
                return Fragment("Partials/_TaskForm", this);
            }

            FlashMessage = "Please correct the errors and try again.";
            return Page();
        }

        // Simulated error condition for demonstration
        // Type "boom" as the title to trigger this error
        if (Input.Title.Trim().Equals("boom", StringComparison.OrdinalIgnoreCase))
        {
            if (IsHtmx())
            {
                Response.Headers["HX-Retarget"] = "#messages";
                Response.Headers["HX-Reswap"] = "innerHTML";
                return Fragment("Partials/_Error",
                    "Simulated server error. Try a different title (anything except 'boom').");
            }

            throw new InvalidOperationException("Simulated server error.");
        }

        // Create the task
        InMemoryTaskStore.Add(Input.Title);
        Tasks = InMemoryTaskStore.All();

        if (IsHtmx())
        {
            // Trigger form clear after successful creation
            Response.Headers["HX-Trigger"] = "clearForm";
            return Fragment("Partials/_TaskList", Tasks);
        }

        // For traditional requests: redirect (PRG pattern)
        FlashMessage = "Task added.";
        return RedirectToPage();
    }

    public IActionResult OnPostReset()
    {
        InMemoryTaskStore.Reset();
        FlashMessage = "Tasks reset.";
        return RedirectToPage();
    }

    public class NewTaskInput
    {
        public string Title { get; set; } = "";
    }
}
```

### 2.3 Understanding the Changes

#### The `IsHtmx()` Helper

```csharp
private bool IsHtmx() =>
    Request.Headers.TryGetValue("HX-Request", out var value) && value == "true";
```

This checks for the `HX-Request` header that htmx sends with every request. This is the standard way to detect htmx requests.

#### The `Fragment()` Helper

```csharp
private PartialViewResult Fragment(string partialName, object model) =>
    new()
    {
        ViewName = partialName,
        ViewData = new ViewDataDictionary(MetadataProvider, ModelState) { Model = model }
    };
```

This creates a `PartialViewResult` that renders just the partial view, not the full page. The `ViewDataDictionary` constructor uses `MetadataProvider` and `ModelState` to preserve validation context needed for showing error messages.

#### The Success Path

```csharp
if (IsHtmx())
{
    Response.Headers["HX-Trigger"] = "clearForm";
    return Fragment("Partials/_TaskList", Tasks);
}
```

When htmx submits successfully, we return just the `_TaskList` partial. htmx swaps this into `#task-list` as specified by `hx-target`. We also send the `HX-Trigger` header to fire a custom event that will clear the form.

#### The Validation Error Path

```csharp
if (IsHtmx())
{
    Response.Headers["HX-Retarget"] = "#task-form";
    Response.Headers["HX-Reswap"] = "outerHTML";
    return Fragment("Partials/_TaskForm", this);
}
```

When validation fails:

1. **HX-Retarget**: Overrides the original `hx-target`; swap into `#task-form` instead
2. **HX-Reswap**: Specifies the swap strategy for this response
3. **Return form**: The form fragment includes validation error messages

### 2.4 Why Retargeting?

The form's `hx-target` is `#task-list` (for successful creates). But on validation failure, we want to update the form instead (to show errors). Response headers let us override the target per-response:

| Scenario         | Target                    | Response                        |
|------------------|---------------------------|---------------------------------|
| Success          | `#task-list`              | `_TaskList` partial             |
| Validation error | `#task-form` (retargeted) | `_TaskForm` partial with errors |
| Server error     | `#messages` (retargeted)  | `_Error` fragment               |

### 2.5 Test the Implementation

1. **Build and run** the application
2. **Navigate** to `/Tasks`
3. **Open Network tab** in Developer Tools
4. **Submit a task** with a valid title
5. **Observe**:
   - Request includes `HX-Request: true` header
   - Response is just the list HTML (not a full page)
   - Only `#task-list` updates; no page flash
6. **Submit empty form**
7. **Observe**:
   - Form updates with validation error
   - List remains unchanged

---

## Step 3: Add a "Refresh List" Button Using hx-get (6–8 minutes)

Now let's add buttons that fetch fresh data without submitting a form. This demonstrates `hx-get` for read operations.

### 3.1 Update the Page with Refresh Buttons

Update `Pages/Tasks/Index.cshtml` to include the loading indicator and refresh buttons:

**File: `Pages/Tasks/Index.cshtml` (relevant section)**

```cshtml
<div class="d-flex justify-content-between align-items-center mb-2">
    <h2 class="h5 mb-0">List</h2>

    <div class="d-flex align-items-center gap-2">
        @* Loading indicator - hidden by default, shown during htmx requests *@
        <div id="task-loading"
             class="htmx-indicator spinner-border spinner-border-sm text-secondary"
             role="status">
            <span class="visually-hidden">Loading...</span>
        </div>

        <div class="btn-group btn-group-sm">
            @* Refresh all tasks *@
            <button type="button"
                    class="btn btn-outline-secondary"
                    hx-get="?handler=List"
                    hx-target="#task-list"
                    hx-swap="outerHTML"
                    hx-indicator="#task-loading">
                Refresh All
            </button>

            @* Refresh with limit using hx-vals *@
            <button type="button"
                    class="btn btn-outline-secondary"
                    hx-get="?handler=List"
                    hx-vals='{"take": 5}'
                    hx-target="#task-list"
                    hx-swap="outerHTML"
                    hx-indicator="#task-loading">
                Top 5
            </button>
        </div>
    </div>
</div>
```

### 3.2 Understanding hx-get

| Attribute      | Value             | Purpose                                   |
|----------------|-------------------|-------------------------------------------|
| `hx-get`       | `"?handler=List"` | Send GET request to this URL when clicked |
| `hx-target`    | `"#task-list"`    | Put the response into #task-list          |
| `hx-swap`      | `"outerHTML"`     | Replace the entire element                |
| `hx-indicator` | `"#task-loading"` | Show loading spinner during request       |

This is the canonical "fetch and swap" pattern:

1. User clicks the button
2. htmx sends `GET /Tasks?handler=List`
3. Server returns `_TaskList` partial
4. htmx replaces `#task-list` with the response

### 3.3 Understanding hx-vals

```html
hx-vals='{"take": 5}'
```

| Aspect     | Detail                                                              |
|------------|---------------------------------------------------------------------|
| **Format** | JSON object as a string                                             |
| **Quotes** | Use single quotes for the attribute, double quotes inside JSON      |
| **Result** | Adds `?take=5` to the request URL (for GET) or form data (for POST) |

The `OnGetList(int? take)` handler already accepts this parameter, so the server will limit results to 5 tasks.

### 3.4 Test the Refresh Buttons

1. **Add a few tasks** using the form
2. **Click "Refresh All"**
3. **Observe** in Network tab:
   - GET request to `?handler=List`
   - Response is just the list HTML
   - List updates without page reload
4. **Click "Top 5"**
5. **Check Network tab**: Request URL should include `?handler=List&take=5`

---

## Step 4: Add Loading Indicator Styling (5–7 minutes)

The loading indicator element is already in place, but we need CSS to show/hide it properly.

### 4.1 Add CSS for the Indicator

The htmx indicator styles should already be in `wwwroot/css/site.css`:

**File: `wwwroot/css/site.css` (relevant section)**

```css
/* ═══════════════════════════════════════════════════════════════
   htmx Loading Indicator Styles
   ═══════════════════════════════════════════════════════════════ */

/* Hide indicator by default */
.htmx-indicator {
    display: none;
}

/* Show indicator when htmx request is in progress */
.htmx-request .htmx-indicator,
.htmx-request.htmx-indicator {
    display: inline-block;
}

/* Optional: Add a subtle opacity transition to the target during loading */
.htmx-request #task-list {
    opacity: 0.5;
    transition: opacity 200ms ease-in-out;
}

/* Disable form elements during submission */
.htmx-request input,
.htmx-request button,
.htmx-request select,
.htmx-request textarea {
    pointer-events: none;
    opacity: 0.7;
}
```

### 4.2 Understanding hx-indicator

| Aspect                 | Detail                                               |
|------------------------|------------------------------------------------------|
| `hx-indicator`         | CSS selector for the indicator element               |
| `htmx-indicator` class | Added to indicator elements; CSS hides it by default |
| `htmx-request` class   | Added to the triggering element during requests      |

htmx's built-in behavior:

1. Request starts → adds `htmx-request` class to the element with `hx-*` attributes
2. CSS rule `.htmx-request .htmx-indicator` shows the indicator
3. Request ends → removes `htmx-request` class
4. Indicator hides again

### 4.3 Test Everything

**Test the loading indicator:**

1. Open Network tab, enable throttling (Slow 3G)
2. Click refresh or submit a task
3. Observe the spinner appears during the request
4. Notice the task list becomes slightly transparent

**Test validation errors:**

1. Submit an empty form
2. Observe form updates with error message
3. List remains unchanged

**Test server error:**

1. Type "boom" as the task title
2. Submit the form
3. Observe error message appears in the messages area

---

## Step 5: Clear the Form After Success (6–8 minutes)

Currently, after successfully adding a task, the form retains the entered value. Let's add a mechanism to clear it using htmx events.

### 5.1 The Strategy: Use HX-Trigger

htmx can fire custom events that other elements listen to. We'll:

1. Server sends `HX-Trigger: clearForm` header on success
2. An invisible listener element catches this and refreshes the form

### 5.2 The Handler is Already There

The `OnGetEmptyForm()` handler is already implemented in the PageModel:

```csharp
public IActionResult OnGetEmptyForm()
{
    Input = new NewTaskInput();
    ModelState.Clear();
    return Fragment("Partials/_TaskForm", this);
}
```

### 5.3 Add a Listener Element to the Page

The listener should already be in `Pages/Tasks/Index.cshtml` (near the bottom, before `@section Scripts`):

**File: `Pages/Tasks/Index.cshtml` (listener section)**

```cshtml
@*
    Invisible listeners for htmx events
    These elements respond to HX-Trigger headers from the server
*@
<div hx-get="?handler=EmptyForm"
     hx-trigger="clearForm from:body"
     hx-target="#task-form"
     hx-swap="outerHTML">
</div>
```

### 5.4 The Event is Already Triggered

The success path in `OnPostCreate` already includes the trigger:

```csharp
if (IsHtmx())
{
    Response.Headers["HX-Trigger"] = "clearForm";
    return Fragment("Partials/_TaskList", Tasks);
}
```

### 5.5 Understanding HX-Trigger

| Aspect                             | Detail                                                                                |
|------------------------------------|---------------------------------------------------------------------------------------|
| `HX-Trigger` header                | Tells htmx to fire a custom event                                                     |
| `hx-trigger="clearForm from:body"` | Listen for "clearForm" event on body                                                  |
| Flow                               | Response includes header → htmx fires event → listener catches it → makes new request |

This pattern keeps your markup clean—the form itself doesn't need to know about clearing. The server controls the behavior through headers.

---

## Complete File Reference

### Index.cshtml (Complete)

**File: `Pages/Tasks/Index.cshtml`**

```cshtml
@page
@model RazorPagesHtmxWorkshop.Pages.Tasks.IndexModel
@{
    ViewData["Title"] = "Tasks • htmx Razor Pages Workshop";
}

<div class="d-flex flex-column flex-md-row align-items-md-end justify-content-between gap-2 mb-3">
    <div>
        <h1 class="mb-1">Tasks</h1>
        <p class="text-muted mb-0">Lab-friendly page with clear fragment boundaries.</p>
    </div>
    <form method="post" asp-page-handler="Reset" class="m-0">
        <button class="btn btn-sm btn-outline-secondary" type="submit">Reset</button>
    </form>
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

                <div class="d-flex justify-content-between align-items-center mb-2">
                    <h2 class="h5 mb-0">List</h2>

                    <div class="d-flex align-items-center gap-2">
                        @* Loading indicator - hidden by default, shown during htmx requests *@
                        <div id="task-loading"
                             class="htmx-indicator spinner-border spinner-border-sm text-secondary"
                             role="status">
                            <span class="visually-hidden">Loading...</span>
                        </div>

                        <div class="btn-group btn-group-sm">
                            @* Refresh all tasks *@
                            <button type="button"
                                    class="btn btn-outline-secondary"
                                    hx-get="?handler=List"
                                    hx-target="#task-list"
                                    hx-swap="outerHTML"
                                    hx-indicator="#task-loading">
                                Refresh All
                            </button>

                            @* Refresh with limit using hx-vals *@
                            <button type="button"
                                    class="btn btn-outline-secondary"
                                    hx-get="?handler=List"
                                    hx-vals='{"take": 5}'
                                    hx-target="#task-list"
                                    hx-swap="outerHTML"
                                    hx-indicator="#task-loading">
                                Top 5
                            </button>
                        </div>
                    </div>
                </div>

                <partial name="Partials/_TaskList" model="Model.Tasks" />
            </div>
        </div>
    </div>
</div>

@*
    Invisible listeners for htmx events
    These elements respond to HX-Trigger headers from the server
*@
<div hx-get="?handler=EmptyForm"
     hx-trigger="clearForm from:body"
     hx-target="#task-form"
     hx-swap="outerHTML">
</div>

@section Scripts {
    <partial name="_ValidationScriptsPartial" />
}
```

### Partials Reference

**File: `Pages/Tasks/Partials/_TaskList.cshtml`**

```cshtml
@using RazorPagesHtmxWorkshop.Models
@model IReadOnlyList<TaskItem>

@*
    Task List Fragment
    ==================

    Target ID: #task-list
    Swap: outerHTML
    Returned by: OnGetList, OnPostCreate (on success)

    This fragment displays the list of tasks.
    The wrapper div with id="task-list" is essential for htmx targeting.
    Using outerHTML swap means the entire div is replaced, not just its contents.
*@

<div id="task-list">
    @if (Model.Count == 0)
    {
        <div class="text-muted">
            No tasks yet. Add one to establish a baseline.
        </div>
    }
    else
    {
        <ul class="list-group list-group-flush">
            @foreach (var task in Model)
            {
                <li class="list-group-item d-flex justify-content-between align-items-center py-3">
                    <div class="d-flex flex-column">
                        <strong>@task.Title</strong>
                        <span class="small text-muted">Created @task.CreatedUtc.ToLocalTime().ToString("g")</span>
                    </div>
                    @if (task.IsDone)
                    {
                        <span class="badge text-bg-success">Done</span>
                    }
                    else
                    {
                        <span class="badge text-bg-secondary">Open</span>
                    }
                </li>
            }
        </ul>
    }
</div>
```

**File: `Pages/Tasks/Partials/_Messages.cshtml`**

```cshtml
@model string?

@*
    Messages Fragment
    =================

    Target ID: #messages
    Swap: innerHTML (when used as error target)

    This fragment displays flash messages and can also receive
    error content via HX-Retarget from server error responses.
*@

<div id="messages">
    @if (!string.IsNullOrWhiteSpace(Model))
    {
        <div class="alert alert-info workshop-alert" role="alert">
            @Model
        </div>
    }
</div>
```

**File: `Pages/Tasks/Partials/_Error.cshtml`**

```cshtml
@model string

@*
    Error Fragment
    ==============

    Purpose:
    - Displays error messages in a consistent format
    - Used for server errors, not found conditions, etc.
    - Typically swapped into #messages via HX-Retarget

    Model:
    - string message - The error message to display
*@

<div class="alert alert-danger workshop-alert" role="alert">
    <strong>Error:</strong> @Model
</div>
```

---

## Verification Checklist

Before moving to Lab 3, verify these behaviors:

### Form Submission

- [ ] Submit with valid title updates only `#task-list` (no page reload)
- [ ] Submit with empty title shows error in `#task-form` (retargeted)
- [ ] Submit with "boom" shows error in `#messages` (retargeted)
- [ ] Form clears after successful submit
- [ ] Loading spinner appears during submission

### Refresh Buttons

- [ ] "Refresh All" fetches and displays all tasks
- [ ] "Top 5" fetches with `take=5` parameter
- [ ] Both buttons show loading indicator during request

### Network Verification

- [ ] Requests include `HX-Request: true` header
- [ ] Responses are HTML fragments (not full pages)
- [ ] Success responses include `HX-Trigger: clearForm` header
- [ ] Error responses include `HX-Retarget` and `HX-Reswap` headers

### DOM Verification

- [ ] `#task-list` exists and updates correctly
- [ ] `#task-form` exists and shows validation errors
- [ ] `#messages` exists and shows server errors
- [ ] Loading indicator shows/hides correctly

---

## Key Takeaways

### The htmx Mental Model

1. **HTML is the response format**: Server returns fragments, not JSON
2. **Targets are CSS selectors**: `hx-target="#task-list"` finds the element by ID
3. **Swap strategies matter**: `outerHTML` replaces, `innerHTML` fills
4. **Headers control behavior**: Response headers can override client attributes

### Patterns You've Learned

| Pattern                    | Usage                                |
|----------------------------|--------------------------------------|
| `hx-post` with `hx-target` | Submit form, update specific region  |
| `hx-get` for refresh       | Fetch fresh data on demand           |
| `hx-vals`                  | Pass parameters without forms        |
| `hx-indicator`             | Show loading feedback                |
| `HX-Retarget`              | Route responses to different targets |
| `HX-Trigger`               | Fire events for secondary actions    |

### Important Implementation Details

1. **Namespace**: Use `RazorPagesHtmxWorkshop` (not `RazorHtmxWorkshop`)
2. **Fragment helper**: Uses `MetadataProvider` and `ModelState` for proper validation context
3. **Progressive enhancement**: Keep traditional attributes (`method`, `asp-page-handler`) as fallbacks
4. **Workshop styling**: Custom CSS provides dark theme with gradient backgrounds

### What Comes Next

In **Lab 3**, you'll implement:

- Real-time validation ("validate as you type")
- Data annotations for validation rules
- Field-level error fragments
- Antiforgery token handling with htmx

---

## Troubleshooting

### Common Issues and Solutions

| Problem                      | Likely Cause                   | Solution                                        |
|------------------------------|--------------------------------|-------------------------------------------------|
| Nothing happens on submit    | htmx not loaded                | Check console for htmx object                   |
| Full page reloads            | htmx not intercepting          | Verify `hx-post` attribute is present           |
| Wrong content swapped in     | Incorrect `hx-target`          | Check selector matches element ID               |
| Nested duplicate elements    | Using `innerHTML` with wrapper | Change to `outerHTML`                           |
| Validation errors don't show | Retarget headers missing       | Add `HX-Retarget` and `HX-Reswap`               |
| Indicator doesn't show       | CSS missing                    | Add `.htmx-indicator` and `.htmx-request` rules |
| Form doesn't clear           | Event listener missing         | Check for listener div at bottom of page        |

### Debug Tips

1. **Network Tab**: Check request headers for `HX-Request: true`
2. **Network Tab**: Check response headers for `HX-Retarget`, `HX-Reswap`, `HX-Trigger`
3. **Console**: Look for htmx errors or warnings
4. **Elements Tab**: Watch DOM changes during swaps
5. **Response Preview**: Verify server returns HTML fragment, not full page

---

## Summary

You have successfully completed Lab 2! Your application now:

- ✅ Submits forms via htmx without page reloads
- ✅ Updates only the affected regions of the page
- ✅ Shows loading indicators during requests
- ✅ Handles validation errors gracefully with retargeting
- ✅ Handles server errors with appropriate feedback
- ✅ Supports parameterized queries with `hx-vals`
- ✅ Clears the form after successful submission

This is the core htmx workflow that you'll use throughout your applications. In Lab 3, you'll build on this foundation to add real-time validation and more sophisticated form handling.

**Proceed to Lab 3: Real-Time Validation and Form UX →**
