---
order: 1
icon: code-square
---

# Lab 3: Real-Time Validation and Form UX

## Overview

In this lab, you will enhance your Task form with real-time validation—providing instant feedback as users type. This is one of the most impactful UX improvements htmx enables: validation that feels responsive without sacrificing server-side authority.

By the end of this lab, your form will:

- Validate input as the user types (with debouncing)
- Show field-level errors without page reload
- Submit with full validation and display a summary on failure
- Show success messages and optionally clear the form

### The Key Insight

Traditional forms validate only on submit. Real-time validation requires either:

1. **Client-side JavaScript** with duplicated validation rules, or
2. **htmx** with server-rendered validation fragments

We'll use htmx to keep validation rules in one place (the server) while delivering instant feedback.

### Two Granularities of Validation

| Type | When | What Updates | Fragment |
|------|------|--------------|----------|
| **Micro validation** | As you type (debounced) | Single field error | `_TitleValidation` |
| **Full validation** | On submit | Entire form with summary | `_TaskForm` |

This dual approach gives users immediate feedback on individual fields while ensuring the full form is validated before submission.

---

## Lab Outcomes

By the end of Lab 3, you will be able to:

| Outcome | Description |
|---------|-------------|
| **Data annotations** | Use `[Required]`, `[StringLength]` for validation rules |
| **Validate as you type** | Implement `hx-trigger="keyup changed delay:500ms"` |
| **Field-level fragments** | Create tiny fragments for individual field errors |
| **Full form validation** | Return entire form fragment with validation summary |
| **Antiforgery handling** | Ensure POST requests include the token |
| **Success messaging** | Use `HX-Trigger` to update messages after success |
| **Form reset** | Optionally clear the form after successful submission |

---

## Prerequisites

Before starting this lab, ensure you have:

- **Completed Lab 2** with all verifications passing
- **Checkpoint complete** with `IsHtmx()` and `Fragment()` helpers in place
- **Working form submission** that updates `#task-list` on success
- **Working retargeting** that updates `#task-form` on validation failure

---

## Step 1: Add Data Annotations to the Input Model (5–7 minutes)

Currently, validation is handled with manual `if` statements. Let's replace that with data annotations—the standard .NET approach.

### 1.1 Understanding Data Annotations

Data annotations are attributes that define validation rules declaratively:

| Attribute | Purpose | Example |
|-----------|---------|---------|
| `[Required]` | Field must have a value | `[Required(ErrorMessage = "Title is required.")]` |
| `[StringLength]` | Min/max length constraints | `[StringLength(60, MinimumLength = 3)]` |
| `[Range]` | Numeric range | `[Range(1, 100)]` |
| `[EmailAddress]` | Valid email format | `[EmailAddress]` |
| `[RegularExpression]` | Custom pattern | `[RegularExpression(@"^[A-Z].*")]` |

### 1.2 Update the NewTaskInput Class

Edit `Pages/Tasks/Index.cshtml.cs` and add annotations to `NewTaskInput`:

**File: `Pages/Tasks/Index.cshtml.cs`**

```csharp
using System.ComponentModel.DataAnnotations;

// Inside IndexModel class:

public class NewTaskInput
{
    [Required(ErrorMessage = "Title is required.")]
    [StringLength(60, MinimumLength = 3, ErrorMessage = "Title must be 3–60 characters.")]
    public string Title { get; set; } = "";
}
```

### 1.3 Understanding the Annotations

| Annotation | Rule | Error Message |
|------------|------|---------------|
| `[Required]` | Cannot be null/empty | "Title is required." |
| `[StringLength(60, MinimumLength = 3)]` | 3–60 characters | "Title must be 3–60 characters." |

**Why Annotations Over Manual Checks:**

1. **Single source of truth**: Rules defined once, used everywhere
2. **Automatic ModelState integration**: Framework handles validation
3. **Client-side validation support**: Can generate JavaScript validation (optional)
4. **Consistent error messages**: Defined alongside the rule

### 1.4 Update OnPostCreate to Use TryValidateModel

Replace manual validation checks with `TryValidateModel`:

**Update `OnPostCreate` in `Index.cshtml.cs`:**

```csharp
public IActionResult OnPostCreate()
{
    // TryValidateModel evaluates all data annotations on Input
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

    // Simulated error (keep from Lab 2 for testing)
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

    // Success path
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

### 1.5 Understanding TryValidateModel

```csharp
if (!TryValidateModel(Input, nameof(Input)))
```

| Aspect | Detail |
|--------|--------|
| **What it does** | Evaluates all data annotations on the specified model |
| **Returns** | `true` if valid, `false` if any validation fails |
| **Side effect** | Populates `ModelState` with errors |
| **Parameter 1** | The model instance to validate |
| **Parameter 2** | The prefix for error keys (matches `asp-for` binding) |

**Why `nameof(Input)`?**

This ensures error keys like `Input.Title` match what Razor's `asp-validation-for` expects. Without it, error messages might not display correctly.

### 1.6 Test the Changes

1. **Build and run** the application
2. **Try submitting** with an empty title → Should see "Title is required."
3. **Try submitting** with "ab" (2 characters) → Should see "Title must be 3–60 characters."
4. **Try submitting** with a valid title → Should succeed

---

## Step 2: Create a Field-Level Validation Fragment (5–7 minutes)

Now we'll create a tiny fragment specifically for the Title field's validation message. This enables real-time feedback without replacing the entire form.

### 2.1 Design the Fragment

The fragment needs:

- A **stable wrapper** with ID `#title-validation`
- **Conditional content**: Show error if present, empty div if valid
- **Minimal size**: Just the error message, nothing else

### 2.2 Create the Validation Partial

**File: `Pages/Tasks/Partials/_TitleValidation.cshtml`**

```cshtml
@model string?

@*
    Title Field Validation Fragment
    ================================

    Purpose:
    - Displays validation error for the Title field
    - Swapped on every keystroke (debounced)
    - Must always render the wrapper div for consistent swapping

    Model:
    - string? error - The error message (null if valid)

    Design notes:
    - Wrapper div renders even when empty (htmx needs stable target)
    - Error styling matches Bootstrap conventions
    - Kept intentionally minimal for fast responses
*@

<div id="title-validation">
    @if (!string.IsNullOrWhiteSpace(Model))
    {
        <div class="text-danger small mt-1">@Model</div>
    }
</div>
```

### 2.3 Understanding the Fragment Design

**Why the wrapper always renders:**

```html
<!-- Valid state (no error) -->
<div id="title-validation"></div>

<!-- Invalid state (has error) -->
<div id="title-validation">
    <div class="text-danger small mt-1">Title is required.</div>
</div>
```

htmx needs a consistent target element. If we returned nothing when valid, htmx wouldn't know what to swap.

**Why `string?` as the model:**

This is the simplest possible model—just the error message or null. The fragment doesn't need the full form context; it only displays one piece of information.

---

## Step 3: Add a Validation Handler (8–10 minutes)

Now we'll create a handler specifically for validating the Title field. This handler is intentionally narrow—it validates one field and returns one fragment.

### 3.1 Add the Validation Handler

**Add to `Pages/Tasks/Index.cshtml.cs`:**

```csharp
/// <summary>
/// Validates the Title field and returns just the validation fragment.
/// Called via htmx on keystrokes (debounced).
///
/// Design: This handler is intentionally "micro"—one field, one fragment.
/// It avoids returning the entire form on each keystroke.
/// </summary>
public IActionResult OnPostValidateTitle()
{
    // Get the current title value (may be null or empty)
    var title = Input.Title?.Trim() ?? "";

    // Manual validation to match data annotations
    // (We could use ModelState, but this is clearer for teaching)
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

    // Return only the tiny validation fragment
    return Fragment("Partials/_TitleValidation", error);
}
```

### 3.2 Understanding the Handler Design

**Why manual validation instead of ModelState?**

For this micro-validation handler, explicit checks are clearer and more predictable:

| Approach | Pros | Cons |
|----------|------|------|
| Manual checks | Clear, explicit, easy to debug | Rules duplicated from annotations |
| ModelState | Uses existing annotations | More complex to extract single-field error |

For field-level validation, manual checks are simpler. The full submit still uses annotations via `TryValidateModel`.

**Why keep it narrow?**

```csharp
// GOOD: Returns tiny fragment
return Fragment("Partials/_TitleValidation", error);

// BAD: Returns entire form (wasteful for keystrokes)
return Fragment("Partials/_TaskForm", this);
```

Keystroke validation fires frequently. Returning the entire form on each keystroke would be wasteful and could cause focus/scroll issues.

### 3.3 Alternative: Using ModelState

If you prefer to use ModelState (to avoid duplicating validation logic):

```csharp
public IActionResult OnPostValidateTitle()
{
    // Clear other errors, validate only Title
    ModelState.Clear();
    TryValidateModel(Input.Title, $"{nameof(Input)}.{nameof(Input.Title)}");

    // Extract error for this field
    string? error = null;
    if (ModelState.TryGetValue("Input.Title", out var entry) && entry.Errors.Count > 0)
    {
        error = entry.Errors[0].ErrorMessage;
    }

    return Fragment("Partials/_TitleValidation", error);
}
```

This approach uses the same annotations but requires more plumbing. For workshops, the manual approach is clearer.

---

## Step 4: Wire Up Real-Time Validation (10–12 minutes)

Now we connect the Title input to the validation handler using htmx attributes.

### 4.1 Update the Form Partial

Edit `Pages/Tasks/Partials/_TaskForm.cshtml` to add validation attributes and the placeholder fragment:

**File: `Pages/Tasks/Partials/_TaskForm.cshtml`**

```cshtml
@model RazorHtmxWorkshop.Pages.Tasks.IndexModel

@*
    Task Form Fragment (with real-time validation)
    ==============================================

    htmx attributes on form:
    - hx-post: Submit to Create handler
    - hx-target: Update #task-list on success
    - hx-swap: Replace entire target element
    - hx-indicator: Show loading spinner

    htmx attributes on Title input:
    - hx-post: Validate on keystroke
    - hx-trigger: Debounced keyup (500ms delay)
    - hx-target: Update only #title-validation
    - hx-include: Send form fields (for antiforgery)

    Progressive enhancement:
    - Form works without JavaScript (method="post" fallback)
    - htmx adds real-time validation on top
*@

<div id="task-form">
    <form method="post" asp-page-handler="Create"
          hx-post="?handler=Create"
          hx-target="#task-list"
          hx-swap="outerHTML"
          hx-indicator="#task-loading">

        @* Antiforgery token - required for all POST requests *@
        @Html.AntiForgeryToken()

        @* Validation summary for full-form validation *@
        <div asp-validation-summary="ModelOnly" class="text-danger mb-3"></div>

        <div class="mb-3">
            <label class="form-label" for="title">Task Title</label>

            @* Title input with real-time validation *@
            <input id="title"
                   class="form-control"
                   asp-for="Input.Title"
                   placeholder="e.g., Add htmx to Razor Pages"
                   hx-post="?handler=ValidateTitle"
                   hx-trigger="keyup changed delay:500ms"
                   hx-target="#title-validation"
                   hx-swap="outerHTML"
                   hx-include="closest form" />

            @* Standard Razor validation message (shown on full submit) *@
            <span class="text-danger" asp-validation-for="Input.Title"></span>

            @* htmx validation fragment (shown on keystrokes) *@
            <partial name="Partials/_TitleValidation" model="@((string?)null)" />
        </div>

        <button class="btn btn-primary" type="submit">Add Task</button>
    </form>
</div>
```

### 4.2 Understanding the htmx Attributes

#### On the Title Input

| Attribute | Value | Purpose |
|-----------|-------|---------|
| `hx-post` | `"?handler=ValidateTitle"` | Send POST to validation handler |
| `hx-trigger` | `"keyup changed delay:500ms"` | Fire after 500ms of no typing |
| `hx-target` | `"#title-validation"` | Update only the validation fragment |
| `hx-swap` | `"outerHTML"` | Replace the entire fragment element |
| `hx-include` | `"closest form"` | Include form fields (especially antiforgery token) |

#### Understanding `hx-trigger`

```html
hx-trigger="keyup changed delay:500ms"
```

| Part | Meaning |
|------|---------|
| `keyup` | Fire on key release |
| `changed` | Only if value actually changed |
| `delay:500ms` | Wait 500ms after last keystroke (debounce) |

**Why debounce?**

Without delay, every keystroke fires a request. With 500ms delay:

- User types "Hello" quickly → 1 request (after they pause)
- User types slowly → Multiple requests (one per pause)

This balances responsiveness with server load.

#### Understanding `hx-include`

```html
hx-include="closest form"
```

When `hx-post` is on an input (not a form), htmx doesn't automatically include sibling form fields. `hx-include` tells htmx to serialize and include fields from the closest form.

**Critical for antiforgery**: Without this, the POST request won't include `__RequestVerificationToken`, causing a 400 or 403 error.

### 4.3 Two Validation Displays

Notice we have both:

```cshtml
@* Standard Razor validation (full submit) *@
<span asp-validation-for="Input.Title"></span>

@* htmx validation (keystrokes) *@
<partial name="Partials/_TitleValidation" model="..." />
```

**Why both?**

| Element | When It Shows | Source |
|---------|---------------|--------|
| `asp-validation-for` | Full form submit | ModelState from server |
| `#title-validation` | Keystroke validation | OnPostValidateTitle handler |

The `asp-validation-for` provides fallback for non-htmx scenarios. The htmx fragment provides real-time feedback.

### 4.4 Test Real-Time Validation

1. **Navigate** to `/Tasks`
2. **Open Network tab** in DevTools
3. **Start typing** in the Title field
4. **Wait 500ms** after typing
5. **Observe**:
   - POST request to `?handler=ValidateTitle`
   - Response is the tiny `#title-validation` fragment
   - Error appears below the input (if invalid)
6. **Continue typing** to fix the error
7. **Observe**:
   - Another request after 500ms
   - Fragment updates to empty (no error)

---

## Step 5: Add Validation Summary to Form Fragment (5–7 minutes)

When the full form submits with errors, we want to show a summary at the top of the form.

### 5.1 Understanding the Validation Summary

The `asp-validation-summary` tag helper renders a list of all validation errors:

| Mode | What It Shows |
|------|---------------|
| `All` | All errors (model-level + property-level) |
| `ModelOnly` | Only model-level errors (not individual properties) |
| `None` | Nothing |

For our form, `ModelOnly` works well because property errors are shown next to each field.

### 5.2 Verify the Summary Is in Place

The form partial should already have:

```cshtml
<div asp-validation-summary="ModelOnly" class="text-danger mb-3"></div>
```

This renders model-level errors when the form is returned after validation failure.

### 5.3 How Full-Form Validation Works

When you submit an invalid form:

1. `OnPostCreate` calls `TryValidateModel`
2. Validation fails, `ModelState` is populated with errors
3. Handler returns the `_TaskForm` fragment (retargeted)
4. htmx swaps the entire form into `#task-form`
5. Razor renders error messages from `ModelState`

The form fragment "knows" about errors because we pass `this` (the PageModel) which includes `ModelState`.

### 5.4 Test Full-Form Validation

1. **Submit** with an empty title
2. **Observe**: Form replaces with error message in `asp-validation-for`
3. **Submit** with "ab" (too short)
4. **Observe**: Form replaces with length error

---

## Step 6: Ensure Antiforgery Works Correctly (5–7 minutes)

Antiforgery tokens prevent cross-site request forgery (CSRF) attacks. Every POST request in ASP.NET Core requires this token.

### 6.1 Where the Token Comes From

```cshtml
@Html.AntiForgeryToken()
```

This renders a hidden input:

```html
<input name="__RequestVerificationToken" type="hidden" value="CfDJ8..." />
```

### 6.2 How htmx Sends the Token

For the form submit (`hx-post` on `<form>`):
- htmx automatically serializes form fields, including the token

For the input validation (`hx-post` on `<input>`):
- htmx only sends the input's value by default
- `hx-include="closest form"` adds all form fields, including the token

### 6.3 Verify Token Flow

1. **Open Network tab**
2. **Type in the Title field** (trigger validation)
3. **Inspect the request payload**:

   ```
   Input.Title=test&__RequestVerificationToken=CfDJ8...
   ```

4. **If token is missing**: You'll see a 400 Bad Request or 403 Forbidden

### 6.4 Troubleshooting Antiforgery Issues

| Symptom | Cause | Fix |
|---------|-------|-----|
| 400 Bad Request | Token missing from request | Add `hx-include="closest form"` |
| 403 Forbidden | Token invalid or expired | Ensure token is in form, refresh page |
| Works on submit, fails on keystroke | `hx-include` missing from input | Add `hx-include="closest form"` to input |

### 6.5 Alternative: Global Antiforgery Header

Instead of including the token in every request, you can configure htmx to send it as a header:

```html
<!-- In _Layout.cshtml -->
<meta name="csrf-token" content="@Html.AntiForgeryToken()" />

<script>
    document.body.addEventListener('htmx:configRequest', function(evt) {
        evt.detail.headers['RequestVerificationToken'] =
            document.querySelector('meta[name="csrf-token"]').getAttribute('content');
    });
</script>
```

This is more complex but useful for larger applications.

---

## Step 7: Add Success Message and Form Reset (10–12 minutes)

After successfully creating a task, we should:

1. Show a success message
2. Optionally clear the form for the next entry

### 7.1 The Strategy: Use HX-Trigger Events

Instead of returning multiple fragments from one response, we'll use `HX-Trigger` to fire events that other elements respond to.

**The Flow:**

1. `OnPostCreate` succeeds → returns `_TaskList` + sets `HX-Trigger: showMessage,clearForm`
2. htmx swaps `#task-list`
3. htmx fires `showMessage` event → listener fetches and swaps `#messages`
4. htmx fires `clearForm` event → listener fetches and swaps `#task-form`

### 7.2 Add the Messages Handler

**Add to `Index.cshtml.cs`:**

```csharp
/// <summary>
/// Returns the messages fragment.
/// Called by htmx listener when showMessage event fires.
/// </summary>
public IActionResult OnGetMessages()
{
    return Fragment("Partials/_Messages", FlashMessage);
}
```

### 7.3 Add the Empty Form Handler

**Add to `Index.cshtml.cs`:**

```csharp
/// <summary>
/// Returns a reset/empty form fragment.
/// Called by htmx listener when clearForm event fires.
/// </summary>
public IActionResult OnGetEmptyForm()
{
    // Reset the bound input
    Input = new NewTaskInput();

    // Clear any lingering validation state
    ModelState.Clear();

    return Fragment("Partials/_TaskForm", this);
}
```

### 7.4 Update OnPostCreate to Trigger Events

**Update the success path in `OnPostCreate`:**

```csharp
// Success path (inside OnPostCreate)
InMemoryTaskStore.Add(Input.Title);
Tasks = InMemoryTaskStore.All();

if (IsHtmx())
{
    // Set flash message for the messages handler
    FlashMessage = "Task added successfully!";

    // Trigger events for listeners to handle
    // Multiple events separated by commas
    Response.Headers["HX-Trigger"] = "showMessage,clearForm";

    return Fragment("Partials/_TaskList", Tasks);
}

FlashMessage = "Task added.";
return RedirectToPage();
```

### 7.5 Add Listener Elements to the Page

These invisible elements respond to triggered events:

**Add to `Pages/Tasks/Index.cshtml` (at the bottom, before `@section Scripts`):**

```cshtml
@*
    Event Listeners
    ===============

    These invisible elements respond to HX-Trigger events from the server.
    When the server sends "HX-Trigger: showMessage,clearForm", these listeners
    fire their respective requests.

    Pattern benefits:
    - Keeps markup clean (form doesn't need to know about messages)
    - Server controls behavior through headers
    - Each concern is handled independently
*@

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
```

### 7.6 Understanding the Listeners

```html
<div hx-get="?handler=Messages"
     hx-trigger="showMessage from:body"
     hx-target="#messages"
     hx-swap="outerHTML">
</div>
```

| Attribute | Value | Purpose |
|-----------|-------|---------|
| `hx-get` | `"?handler=Messages"` | Fetch the messages fragment |
| `hx-trigger` | `"showMessage from:body"` | Fire when `showMessage` event bubbles to body |
| `hx-target` | `"#messages"` | Swap into the messages region |
| `hx-swap` | `"outerHTML"` | Replace the entire element |

**How it works:**

1. Server includes `HX-Trigger: showMessage` in response headers
2. htmx dispatches a `showMessage` custom event on the body
3. The listener element catches the event (because of `from:body`)
4. Listener fires its `hx-get` request
5. Response swaps into `#messages`

### 7.7 Update the Messages Partial

Ensure `_Messages.cshtml` always renders its wrapper:

**File: `Pages/Tasks/Partials/_Messages.cshtml`**

```cshtml
@model string?

@*
    Messages Fragment
    =================

    Purpose:
    - Displays flash/success messages
    - Must always render wrapper for consistent swapping

    Model:
    - string? message - The message to display (null if none)
*@

<div id="messages">
    @if (!string.IsNullOrWhiteSpace(Model))
    {
        <div class="alert alert-success alert-dismissible fade show" role="alert">
            @Model
            <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
        </div>
    }
</div>
```

### 7.8 Test the Complete Flow

1. **Navigate** to `/Tasks`
2. **Add a valid task** (3+ characters)
3. **Observe**:
   - Task appears in the list
   - Success message appears at the top
   - Form clears (ready for next entry)
4. **Check Network tab**: You should see 3 requests:
   - POST to `?handler=Create` (returns list)
   - GET to `?handler=Messages` (returns success message)
   - GET to `?handler=EmptyForm` (returns clean form)

---

## Step 8: Complete Code Reference

Here is the complete code for all files modified in this lab.

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

    #region Helper Methods

    /// <summary>
    /// Checks if the current request was made by htmx.
    /// </summary>
    private bool IsHtmx() =>
        Request.Headers.TryGetValue("HX-Request", out var value) && value == "true";

    /// <summary>
    /// Returns a partial view result for fragment responses.
    /// </summary>
    private PartialViewResult Fragment(string partialName, object model) =>
        new()
        {
            ViewName = partialName,
            ViewData = new ViewDataDictionary(ViewData) { Model = model }
        };

    #endregion

    #region Page Handlers

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

    /// <summary>
    /// Validates the Title field and returns just the validation fragment.
    /// Called via htmx on keystrokes (debounced).
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
            Response.Headers["HX-Trigger"] = "showMessage,clearForm";
            return Fragment("Partials/_TaskList", Tasks);
        }

        FlashMessage = "Task added.";
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

### _TaskForm.cshtml (Complete)

**File: `Pages/Tasks/Partials/_TaskForm.cshtml`**

```cshtml
@model RazorHtmxWorkshop.Pages.Tasks.IndexModel

@*
    Task Form Fragment (with real-time validation)
    ==============================================

    Features:
    - Real-time validation on Title field (debounced 500ms)
    - Full form validation on submit
    - Antiforgery token for security
    - Loading indicator support
    - Progressive enhancement (works without JavaScript)
*@

<div id="task-form">
    <form method="post" asp-page-handler="Create"
          hx-post="?handler=Create"
          hx-target="#task-list"
          hx-swap="outerHTML"
          hx-indicator="#task-loading">

        @Html.AntiForgeryToken()

        <div asp-validation-summary="ModelOnly" class="text-danger mb-3"></div>

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

        <button class="btn btn-primary" type="submit">Add Task</button>
    </form>
</div>
```

### _TitleValidation.cshtml (Complete)

**File: `Pages/Tasks/Partials/_TitleValidation.cshtml`**

```cshtml
@model string?

@*
    Title Field Validation Fragment
    ================================

    Purpose: Display validation error for Title field
    Model: string? error message (null if valid)

    Note: Wrapper must always render for consistent htmx swapping
*@

<div id="title-validation">
    @if (!string.IsNullOrWhiteSpace(Model))
    {
        <div class="text-danger small mt-1">@Model</div>
    }
</div>
```

### _Messages.cshtml (Complete)

**File: `Pages/Tasks/Partials/_Messages.cshtml`**

```cshtml
@model string?

@*
    Messages Fragment
    =================

    Purpose: Display flash/success/info messages
    Model: string? message (null if none)

    Note: Wrapper must always render for consistent htmx swapping
*@

<div id="messages">
    @if (!string.IsNullOrWhiteSpace(Model))
    {
        <div class="alert alert-success alert-dismissible fade show" role="alert">
            @Model
            <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
        </div>
    }
</div>
```

### Index.cshtml (Updated Sections)

**Add at the bottom of `Pages/Tasks/Index.cshtml` (before `@section Scripts`):**

```cshtml
@*
    Event Listeners
    ===============

    Invisible elements that respond to HX-Trigger events
*@

<div hx-get="?handler=Messages"
     hx-trigger="showMessage from:body"
     hx-target="#messages"
     hx-swap="outerHTML">
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

## Verification Checklist

Before moving to Lab 4, verify these behaviors:

### Real-Time Validation

- [ ] Typing into Title triggers a request after 500ms idle
- [ ] Response swaps only `#title-validation` (tiny fragment)
- [ ] Error appears when title is empty or too short
- [ ] Error disappears when title becomes valid

### Full-Form Validation

- [ ] Submitting invalid form swaps entire `#task-form`
- [ ] Validation summary shows at top of form (if applicable)
- [ ] Field error shows next to Title input

### Antiforgery

- [ ] Keystroke validation requests include `__RequestVerificationToken`
- [ ] Form submit requests include `__RequestVerificationToken`
- [ ] No 400 or 403 errors on POST requests

### Success Flow

- [ ] Successful submit updates `#task-list` with new task
- [ ] Success message appears in `#messages`
- [ ] Form clears and is ready for next entry

### Network Verification

- [ ] Keystroke validation fires POST to `?handler=ValidateTitle`
- [ ] Form submit fires POST to `?handler=Create`
- [ ] Success triggers GET to `?handler=Messages` and `?handler=EmptyForm`

---

## Key Takeaways

### Two Granularities of Feedback

| Granularity | Trigger | Target | Fragment | Use Case |
|-------------|---------|--------|----------|----------|
| **Micro** | Keystroke (debounced) | Field container | `_TitleValidation` | Instant feedback |
| **Full** | Form submit | Form container | `_TaskForm` | Complete validation |

### Patterns You've Learned

| Pattern | Implementation |
|---------|----------------|
| Debounced validation | `hx-trigger="keyup changed delay:500ms"` |
| Field-level fragments | Tiny partials with stable wrapper IDs |
| Include form fields | `hx-include="closest form"` for antiforgery |
| Event-driven updates | `HX-Trigger` header + listener elements |
| Data annotations | `[Required]`, `[StringLength]` on input models |

### When to Use Which Approach

| Scenario | Approach | Why |
|----------|----------|-----|
| Single field feedback | Micro validation | Fast, focused, non-disruptive |
| Form submission | Full validation | Complete check before persist |
| Success actions | HX-Trigger events | Decouple concerns, clean markup |
| Error display | Retarget to form | Show all errors in context |

---

## Troubleshooting

### Common Issues and Solutions

| Problem | Likely Cause | Solution |
|---------|--------------|----------|
| Validation fires on every keystroke | Missing `delay:` | Add `delay:500ms` to trigger |
| 400/403 on validation | Antiforgery token missing | Add `hx-include="closest form"` |
| Error doesn't clear | Fragment returns nothing | Always render wrapper div |
| Form doesn't reset | clearForm event not firing | Check `HX-Trigger` header |
| Messages don't appear | showMessage event not firing | Check `HX-Trigger` header |
| Double error messages | Both asp-validation-for and htmx | Style to hide one during typing |

### Debug Tips

1. **Check Network tab**: Verify requests fire at expected times
2. **Check Response headers**: Look for `HX-Trigger`, `HX-Retarget`
3. **Check Console**: Look for htmx errors
4. **Inspect Elements**: Verify fragment IDs match targets

---

## What Comes Next

In **Lab 4**, you'll implement:

- Details view pattern (panel or modal)
- Delete with confirmation
- Filtering and pagination with URL state
- Better swap strategies and transitions

**Proceed to Lab 4: Core UX Patterns (Modal, Confirm, History, Pagination) →**
