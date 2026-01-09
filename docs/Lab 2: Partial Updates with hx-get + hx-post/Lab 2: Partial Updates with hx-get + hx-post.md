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

| Traditional SPA | htmx Approach |
|-----------------|---------------|
| Server returns JSON | Server returns HTML |
| Client renders UI | Server renders UI |
| Complex client state | Simple request/response |
| Heavy JavaScript | Minimal JavaScript |
| Separate API contracts | HTML is the contract |

---

## Lab Outcomes

By the end of Lab 2, you will be able to:

| Outcome | Description |
|---------|-------------|
| **Submit via hx-post** | Create form submits without page reload |
| **Target specific regions** | Update only `#task-list` using `hx-target` |
| **Swap strategies** | Use `hx-swap="outerHTML"` to replace fragments |
| **Refresh via hx-get** | Add a button that fetches fresh data |
| **Pass parameters** | Use `hx-vals` to send additional data |
| **Loading indicators** | Show feedback during requests with `hx-indicator` |
| **Error handling** | Use response headers to retarget error responses |

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

Open `Pages/Shared/_Layout.cshtml` and ensure htmx is included. If not present, add it before the closing `</body>` tag:

**Add to `_Layout.cshtml` (if not already present):**

```html
<!-- htmx library -->
<script src="https://unpkg.com/htmx.org@1.9.12"></script>
```

> **Note**: You can also use a specific CDN or download htmx locally. The unpkg CDN is convenient for workshops.

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
@model RazorHtmxWorkshop.Pages.Tasks.IndexModel

@*
    Task Form Fragment (htmx-enhanced)
    ===================================

    htmx attributes added:
    - hx-post: Send POST request to this URL
    - hx-target: Where to swap the response
    - hx-swap: How to swap (outerHTML replaces the entire target element)

    On successful submit:
    - Server returns _TaskList partial
    - htmx swaps it into #task-list
    - Only the list updates; form and page stay intact
*@

<div id="task-form">
    <form method="post" asp-page-handler="Create"
          hx-post="?handler=Create"
          hx-target="#task-list"
          hx-swap="outerHTML">
        <div class="mb-3">
            <label class="form-label" for="title">Task Title</label>
            <input id="title"
                   class="form-control"
                   asp-for="Input.Title"
                   placeholder="e.g., Add htmx to Razor Pages" />
            <span class="text-danger" asp-validation-for="Input.Title"></span>
        </div>
        <button class="btn btn-primary" type="submit">Add Task</button>
    </form>
</div>
```

### 1.3 Understanding the htmx Attributes

| Attribute | Value | Purpose |
|-----------|-------|---------|
| `hx-post` | `"?handler=Create"` | Send a POST request to this URL when form submits |
| `hx-target` | `"#task-list"` | Put the response into the element with id="task-list" |
| `hx-swap` | `"outerHTML"` | Replace the entire target element (not just its contents) |

**Why keep `method="post"` and `asp-page-handler`?**

These provide **progressive enhancement**. If JavaScript is disabled or htmx fails to load, the form still works as a traditional form. The htmx attributes layer behavior on top without breaking the fallback.

### 1.4 Why `hx-swap="outerHTML"`?

There are several swap strategies:

| Strategy | Behavior |
|----------|----------|
| `innerHTML` | Replace target's children only |
| `outerHTML` | Replace the entire target element |
| `beforebegin` | Insert before the target |
| `afterend` | Insert after the target |
| `beforeend` | Append inside target |
| `afterbegin` | Prepend inside target |

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
            ViewData = new ViewDataDictionary(ViewData) { Model = model }
        };

    public void OnGet()
    {
        Tasks = InMemoryTaskStore.All();
    }

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
                Response.StatusCode = 422; // Unprocessable Entity
                Response.Headers["HX-Retarget"] = "#task-form";
                Response.Headers["HX-Reswap"] = "outerHTML";
                return Fragment("Partials/_TaskForm", this);
            }

            return Page();
        }

        // Create the task
        InMemoryTaskStore.Add(Input.Title);
        Tasks = InMemoryTaskStore.All();

        if (IsHtmx())
        {
            // For htmx: return just the updated list fragment
            return Fragment("Partials/_TaskList", Tasks);
        }

        // For traditional requests: redirect (PRG pattern)
        FlashMessage = "Task added.";
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
        ViewData = new ViewDataDictionary(ViewData) { Model = model }
    };
```

This creates a `PartialViewResult` that renders just the partial view, not the full page. The `ViewDataDictionary` preserves the context (like ModelState) needed for validation messages.

#### The Success Path

```csharp
if (IsHtmx())
{
    return Fragment("Partials/_TaskList", Tasks);
}
```

When htmx submits successfully, we return just the `_TaskList` partial. htmx swaps this into `#task-list` as specified by `hx-target`.

#### The Validation Error Path

```csharp
if (IsHtmx())
{
    Response.StatusCode = 422;
    Response.Headers["HX-Retarget"] = "#task-form";
    Response.Headers["HX-Reswap"] = "outerHTML";
    return Fragment("Partials/_TaskForm", this);
}
```

This is more complex. When validation fails:

1. **Status 422**: Signals "validation error" (Unprocessable Entity)
2. **HX-Retarget**: Overrides the original `hx-target`; swap into `#task-form` instead
3. **HX-Reswap**: Specifies the swap strategy for this response
4. **Return form**: The form fragment includes validation error messages

### 2.4 Why Retargeting?

The form's `hx-target` is `#task-list` (for successful creates). But on validation failure, we want to update the form instead (to show errors). Response headers let us override the target per-response:

| Scenario | Target | Response |
|----------|--------|----------|
| Success | `#task-list` | `_TaskList` partial |
| Validation error | `#task-form` (retargeted) | `_TaskForm` partial with errors |

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
   - Response has status 422
   - Form updates with validation error
   - List remains unchanged

---

## Step 3: Add a "Refresh List" Button Using hx-get (6–8 minutes)

Now let's add a button that fetches fresh data without submitting a form. This demonstrates `hx-get` for read operations.

### 3.1 Add a Handler for List Refresh

Add this handler to `Pages/Tasks/Index.cshtml.cs`:

**Add to `Index.cshtml.cs`:**

```csharp
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
```

### 3.2 Add the Refresh Button to the Page

Update `Pages/Tasks/Index.cshtml` to add a refresh button near the list:

**Update `Pages/Tasks/Index.cshtml`:**

```cshtml
@page
@model RazorHtmxWorkshop.Pages.Tasks.IndexModel
@{
    ViewData["Title"] = "Tasks";
}

<h1>Tasks</h1>

<partial name="Partials/_Messages" model="Model.FlashMessage" />

<div class="row">
    <div class="col-md-5">
        <h2 class="h5">Add a Task</h2>
        <partial name="Partials/_TaskForm" model="Model" />
    </div>

    <div class="col-md-7">
        <div class="d-flex justify-content-between align-items-center mb-2">
            <h2 class="h5 mb-0">Current Tasks</h2>

            @* Refresh button using hx-get *@
            <button type="button"
                    class="btn btn-sm btn-outline-secondary"
                    hx-get="?handler=List"
                    hx-target="#task-list"
                    hx-swap="outerHTML">
                Refresh List
            </button>
        </div>

        <partial name="Partials/_TaskList" model="Model.Tasks" />
    </div>
</div>

@section Scripts {
    <partial name="_ValidationScriptsPartial" />
}
```

### 3.3 Understanding hx-get

| Attribute | Value | Purpose |
|-----------|-------|---------|
| `hx-get` | `"?handler=List"` | Send GET request to this URL when clicked |
| `hx-target` | `"#task-list"` | Put the response into #task-list |
| `hx-swap` | `"outerHTML"` | Replace the entire element |

This is the canonical "fetch and swap" pattern:

1. User clicks the button
2. htmx sends `GET /Tasks?handler=List`
3. Server returns `_TaskList` partial
4. htmx replaces `#task-list` with the response

### 3.4 Test the Refresh Button

1. **Add a few tasks** using the form
2. **Click "Refresh List"**
3. **Observe** in Network tab:
   - GET request to `?handler=List`
   - Response is just the list HTML
   - List updates without page reload

---

## Step 4: Introduce hx-vals for Simple Parameters (5–7 minutes)

Sometimes you need to pass additional data with a request without building a form. The `hx-vals` attribute lets you include JSON data in requests.

### 4.1 Update the Refresh Button with hx-vals

Let's add a button that shows only the top 5 tasks:

**Update the buttons section in `Index.cshtml`:**

```cshtml
<div class="d-flex justify-content-between align-items-center mb-2">
    <h2 class="h5 mb-0">Current Tasks</h2>

    <div class="btn-group btn-group-sm">
        @* Refresh all tasks *@
        <button type="button"
                class="btn btn-outline-secondary"
                hx-get="?handler=List"
                hx-target="#task-list"
                hx-swap="outerHTML">
            Refresh All
        </button>

        @* Refresh with limit using hx-vals *@
        <button type="button"
                class="btn btn-outline-secondary"
                hx-get="?handler=List"
                hx-vals='{"take": 5}'
                hx-target="#task-list"
                hx-swap="outerHTML">
            Top 5
        </button>
    </div>
</div>
```

### 4.2 Understanding hx-vals

```html
hx-vals='{"take": 5}'
```

| Aspect | Detail |
|--------|--------|
| **Format** | JSON object as a string |
| **Quotes** | Use single quotes for the attribute, double quotes inside JSON |
| **Result** | Adds `?take=5` to the request URL (for GET) or form data (for POST) |

The `OnGetList(int? take)` handler already accepts this parameter, so the server will limit results to 5 tasks.

### 4.3 Alternative: Hidden Inputs

For more complex scenarios or when you want the values in the DOM, you can use hidden inputs:

```html
<form hx-get="?handler=List" hx-target="#task-list" hx-swap="outerHTML">
    <input type="hidden" name="take" value="5" />
    <button type="submit" class="btn btn-outline-secondary">Top 5</button>
</form>
```

Both approaches work; `hx-vals` is more concise for simple cases.

### 4.4 Test hx-vals

1. **Add more than 5 tasks** (or just a few to see the difference)
2. **Click "Top 5"**
3. **Check Network tab**: Request URL should include `?handler=List&take=5`
4. **Click "Refresh All"**: Shows all tasks (no `take` parameter)

---

## Step 5: Add Loading Indicator + Error Handling (10–12 minutes)

Real applications need user feedback during requests and graceful error handling. Let's add both.

### 5.1 Add a Loading Indicator Element

First, add a spinner element to the page. Update `Pages/Tasks/Index.cshtml`:

**Update the button section in `Index.cshtml`:**

```cshtml
<div class="d-flex justify-content-between align-items-center mb-2">
    <h2 class="h5 mb-0">Current Tasks</h2>

    <div class="d-flex align-items-center gap-2">
        @* Loading indicator - hidden by default, shown during htmx requests *@
        <div id="task-loading"
             class="htmx-indicator spinner-border spinner-border-sm text-secondary"
             role="status">
            <span class="visually-hidden">Loading...</span>
        </div>

        <div class="btn-group btn-group-sm">
            <button type="button"
                    class="btn btn-outline-secondary"
                    hx-get="?handler=List"
                    hx-target="#task-list"
                    hx-swap="outerHTML"
                    hx-indicator="#task-loading">
                Refresh All
            </button>

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

### 5.2 Update the Form with Indicator

Also update the form partial to use the indicator:

**Update `Pages/Tasks/Partials/_TaskForm.cshtml`:**

```cshtml
@model RazorHtmxWorkshop.Pages.Tasks.IndexModel

<div id="task-form">
    <form method="post" asp-page-handler="Create"
          hx-post="?handler=Create"
          hx-target="#task-list"
          hx-swap="outerHTML"
          hx-indicator="#task-loading">
        <div class="mb-3">
            <label class="form-label" for="title">Task Title</label>
            <input id="title"
                   class="form-control"
                   asp-for="Input.Title"
                   placeholder="e.g., Add htmx to Razor Pages" />
            <span class="text-danger" asp-validation-for="Input.Title"></span>
        </div>
        <button class="btn btn-primary" type="submit">Add Task</button>
    </form>
</div>
```

### 5.3 Add CSS for the Indicator

htmx adds/removes the class `htmx-request` on elements during requests. We need CSS to show/hide the indicator.

Add to `wwwroot/css/site.css`:

**Add to `site.css`:**

```css
/* htmx loading indicator styles */

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
```

### 5.4 Understanding hx-indicator

| Aspect | Detail |
|--------|--------|
| `hx-indicator` | CSS selector for the indicator element |
| `htmx-indicator` class | Added to indicator elements; CSS hides it by default |
| `htmx-request` class | Added to the triggering element during requests |

htmx's built-in behavior:

1. Request starts → adds `htmx-request` class to the element with `hx-*` attributes
2. CSS rule `.htmx-request .htmx-indicator` shows the indicator
3. Request ends → removes `htmx-request` class
4. Indicator hides again

### 5.5 Create an Error Fragment

Create a partial for displaying errors:

**File: `Pages/Tasks/Partials/_Error.cshtml`**

```cshtml
@model string

@*
    Error Fragment
    ==============

    Purpose:
    - Displays error messages in a consistent format
    - Used for server errors, not found conditions, etc.

    Model:
    - string message - The error message to display
*@

<div class="alert alert-danger" role="alert">
    <strong>Error:</strong> @Model
</div>
```

### 5.6 Add Error Handling to OnPostCreate

Update the `OnPostCreate` method to demonstrate error handling:

**Update `OnPostCreate` in `Index.cshtml.cs`:**

```csharp
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
            Response.StatusCode = 422;
            Response.Headers["HX-Retarget"] = "#task-form";
            Response.Headers["HX-Reswap"] = "outerHTML";
            return Fragment("Partials/_TaskForm", this);
        }

        return Page();
    }

    // Simulated error condition for demonstration
    // Type "boom" as the title to trigger this error
    if (Input.Title.Trim().Equals("boom", StringComparison.OrdinalIgnoreCase))
    {
        if (IsHtmx())
        {
            Response.StatusCode = 500;
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
        return Fragment("Partials/_TaskList", Tasks);
    }

    FlashMessage = "Task added.";
    return RedirectToPage();
}
```

### 5.7 Understanding Error Handling

The error handling demonstrates several htmx patterns:

| Scenario | Status | Retarget | Fragment |
|----------|--------|----------|----------|
| Success | 200 | (none) | `_TaskList` → `#task-list` |
| Validation error | 422 | `#task-form` | `_TaskForm` with errors |
| Server error | 500 | `#messages` | `_Error` message |

**Key Points:**

1. **htmx treats non-2xx responses as valid**: You can still return HTML and swap it
2. **HX-Retarget changes where content goes**: Lets you route errors to a different location
3. **HX-Reswap changes how content is swapped**: `innerHTML` here because we want to put content inside `#messages`, not replace it

### 5.8 Test Everything

**Test the loading indicator:**

1. Open Network tab, enable throttling (Slow 3G)
2. Click refresh or submit a task
3. Observe the spinner appears during the request

**Test validation errors:**

1. Submit an empty form
2. Observe form updates with error message
3. List remains unchanged

**Test server error:**

1. Type "boom" as the task title
2. Submit the form
3. Observe error message appears in the messages area

---

## Step 6: Clear the Form After Success (Optional Enhancement)

Currently, after successfully adding a task, the form retains the entered value. Let's add a mechanism to clear it.

### 6.1 The Strategy: Use HX-Trigger

htmx can fire custom events that other elements listen to. We'll:

1. Server sends `HX-Trigger: clearForm` header on success
2. An invisible listener element catches this and refreshes the form

### 6.2 Add a Handler to Return Empty Form

**Add to `Index.cshtml.cs`:**

```csharp
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
```

### 6.3 Add a Listener Element to the Page

**Add to `Index.cshtml` (at the bottom, before `@section Scripts`):**

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

### 6.4 Trigger the Event from OnPostCreate

Update the success path in `OnPostCreate`:

**Update success path in `OnPostCreate`:**

```csharp
if (IsHtmx())
{
    // Trigger form clear after successful creation
    Response.Headers["HX-Trigger"] = "clearForm";
    return Fragment("Partials/_TaskList", Tasks);
}
```

### 6.5 Understanding HX-Trigger

| Aspect | Detail |
|--------|--------|
| `HX-Trigger` header | Tells htmx to fire a custom event |
| `hx-trigger="clearForm from:body"` | Listen for "clearForm" event on body |
| Flow | Response includes header → htmx fires event → listener catches it → makes new request |

This pattern keeps your markup clean—the form itself doesn't need to know about clearing. The server controls the behavior through headers.

---

## Verification Checklist

Before moving to Lab 3, verify these behaviors:

### Form Submission

- [ ] Submit with valid title updates only `#task-list` (no page reload)
- [ ] Submit with empty title shows error in `#task-form` (retargeted)
- [ ] Submit with "boom" shows error in `#messages` (retargeted)
- [ ] Form clears after successful submit (if you implemented Step 6)

### Refresh Buttons

- [ ] "Refresh All" fetches and displays all tasks
- [ ] "Top 5" fetches with `take=5` parameter
- [ ] Both buttons show loading indicator during request

### Network Verification

- [ ] Requests include `HX-Request: true` header
- [ ] Responses are HTML fragments (not full pages)
- [ ] Validation errors return status 422
- [ ] Server errors return status 500

### DOM Verification

- [ ] `#task-list` exists and updates correctly
- [ ] `#task-form` exists and shows validation errors
- [ ] `#messages` exists and shows server errors

---

## Key Takeaways

### The htmx Mental Model

1. **HTML is the response format**: Server returns fragments, not JSON
2. **Targets are CSS selectors**: `hx-target="#task-list"` finds the element by ID
3. **Swap strategies matter**: `outerHTML` replaces, `innerHTML` fills
4. **Headers control behavior**: Response headers can override client attributes

### Patterns You've Learned

| Pattern | Usage |
|---------|-------|
| `hx-post` with `hx-target` | Submit form, update specific region |
| `hx-get` for refresh | Fetch fresh data on demand |
| `hx-vals` | Pass parameters without forms |
| `hx-indicator` | Show loading feedback |
| `HX-Retarget` | Route responses to different targets |
| `HX-Trigger` | Fire events for secondary actions |

### What Comes Next

In **Lab 3**, you'll implement:

- Real-time validation ("validate as you type")
- Data annotations for validation rules
- Field-level error fragments
- Antiforgery token handling with htmx

---

## Troubleshooting

### Common Issues and Solutions

| Problem | Likely Cause | Solution |
|---------|--------------|----------|
| Nothing happens on submit | htmx not loaded | Check console for htmx object |
| Full page reloads | htmx not intercepting | Verify `hx-post` attribute is present |
| Wrong content swapped in | Incorrect `hx-target` | Check selector matches element ID |
| Nested duplicate elements | Using `innerHTML` with wrapper | Change to `outerHTML` |
| Validation errors don't show | Retarget headers missing | Add `HX-Retarget` and `HX-Reswap` |
| Indicator doesn't show | CSS missing | Add `.htmx-indicator` and `.htmx-request` rules |
| 400 Bad Request | Antiforgery token issue | Ensure token is in form (we'll fix properly in Lab 3) |

### Debug Tips

1. **Network Tab**: Check request headers for `HX-Request: true`
2. **Network Tab**: Check response headers for `HX-Retarget`, `HX-Reswap`
3. **Console**: Look for htmx errors or warnings
4. **Elements Tab**: Watch DOM changes during swaps
5. **Response Preview**: Verify server returns HTML fragment, not full page

---

## Complete Code Reference

### Index.cshtml.cs (Complete)

```csharp
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

    private bool IsHtmx() =>
        Request.Headers.TryGetValue("HX-Request", out var value) && value == "true";

    private PartialViewResult Fragment(string partialName, object model) =>
        new()
        {
            ViewName = partialName,
            ViewData = new ViewDataDictionary(ViewData) { Model = model }
        };

    public void OnGet()
    {
        Tasks = InMemoryTaskStore.All();
    }

    public IActionResult OnGetList(int? take)
    {
        var tasks = InMemoryTaskStore.All();

        if (take is > 0)
        {
            tasks = tasks.Take(take.Value).ToList();
        }

        return Fragment("Partials/_TaskList", tasks);
    }

    public IActionResult OnGetEmptyForm()
    {
        Input = new NewTaskInput();
        ModelState.Clear();
        return Fragment("Partials/_TaskForm", this);
    }

    public IActionResult OnPostCreate()
    {
        if (string.IsNullOrWhiteSpace(Input.Title))
        {
            ModelState.AddModelError(nameof(Input.Title), "Title is required.");
        }

        if (!ModelState.IsValid)
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

        if (Input.Title.Trim().Equals("boom", StringComparison.OrdinalIgnoreCase))
        {
            if (IsHtmx())
            {
                Response.StatusCode = 500;
                Response.Headers["HX-Retarget"] = "#messages";
                Response.Headers["HX-Reswap"] = "innerHTML";
                return Fragment("Partials/_Error",
                    "Simulated server error. Try a different title (anything except 'boom').");
            }

            throw new InvalidOperationException("Simulated server error.");
        }

        InMemoryTaskStore.Add(Input.Title);
        Tasks = InMemoryTaskStore.All();

        if (IsHtmx())
        {
            Response.Headers["HX-Trigger"] = "clearForm";
            return Fragment("Partials/_TaskList", Tasks);
        }

        FlashMessage = "Task added.";
        return RedirectToPage();
    }

    public class NewTaskInput
    {
        public string Title { get; set; } = "";
    }
}
```

### Index.cshtml (Complete)

```cshtml
@page
@model RazorHtmxWorkshop.Pages.Tasks.IndexModel
@{
    ViewData["Title"] = "Tasks";
}

<h1>Tasks</h1>

<partial name="Partials/_Messages" model="Model.FlashMessage" />

<div class="row">
    <div class="col-md-5">
        <h2 class="h5">Add a Task</h2>
        <partial name="Partials/_TaskForm" model="Model" />
    </div>

    <div class="col-md-7">
        <div class="d-flex justify-content-between align-items-center mb-2">
            <h2 class="h5 mb-0">Current Tasks</h2>

            <div class="d-flex align-items-center gap-2">
                <div id="task-loading"
                     class="htmx-indicator spinner-border spinner-border-sm text-secondary"
                     role="status">
                    <span class="visually-hidden">Loading...</span>
                </div>

                <div class="btn-group btn-group-sm">
                    <button type="button"
                            class="btn btn-outline-secondary"
                            hx-get="?handler=List"
                            hx-target="#task-list"
                            hx-swap="outerHTML"
                            hx-indicator="#task-loading">
                        Refresh All
                    </button>

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

<div hx-get="?handler=EmptyForm"
     hx-trigger="clearForm from:body"
     hx-target="#task-form"
     hx-swap="outerHTML">
</div>

@section Scripts {
    <partial name="_ValidationScriptsPartial" />
}
```

---

## Summary

You have successfully completed Lab 2! Your application now:

- ✅ Submits forms via htmx without page reloads
- ✅ Updates only the affected regions of the page
- ✅ Shows loading indicators during requests
- ✅ Handles validation errors gracefully with retargeting
- ✅ Handles server errors with appropriate feedback
- ✅ Supports parameterized queries with `hx-vals`

This is the core htmx workflow that you'll use throughout your applications. In Lab 3, you'll build on this foundation to add real-time validation and more sophisticated form handling.

**Proceed to Lab 3: Real-Time Validation and Form UX →**
