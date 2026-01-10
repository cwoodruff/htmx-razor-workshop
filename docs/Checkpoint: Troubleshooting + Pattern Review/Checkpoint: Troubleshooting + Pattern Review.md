---
order: 1
icon: code-square
---

# Checkpoint: Troubleshooting + Pattern Review

## Overview

This checkpoint is a structured pause between Labs 2 and 3. Before moving to more advanced patterns like real-time validation, you need to be confident that:

1. Your development environment works reliably
2. You can debug htmx interactions using browser DevTools
3. You understand the common mistakes and how to fix them
4. You have consistent conventions for handling htmx vs. non-htmx requests

**Time estimate**: 10–15 minutes

---

## Checkpoint Outcomes

By the end of this checkpoint, you will be able to:

| Outcome                          | Description                                                    |
|----------------------------------|----------------------------------------------------------------|
| **Run and debug confidently**    | Locate the right handler for any interaction                   |
| **Use DevTools effectively**     | Verify htmx request/response behavior                          |
| **Recognize common mistakes**    | Identify and fix the top 5 htmx/Razor Pages integration issues |
| **Apply consistent conventions** | Standardize Page vs. Fragment response patterns                |

---

## Part 1: Verify Your Development Loop (5–7 minutes)

Before debugging htmx-specific issues, confirm your basic development workflow is solid.

### 1.1 Run + Debug Checklist

Verify each of these items:

- [ ] Application runs locally (HTTPS or HTTP—either is fine)
- [ ] Breakpoints hit inside handlers:
  - `OnPostCreate`
  - `OnGetList`
- [ ] Hot reload or rebuild works reliably
- [ ] Browser refreshes correctly after code changes

**Debug Exercise:**

Set a breakpoint on the first line of `OnPostCreate`. Submit the form and confirm:

1. The breakpoint is hit
2. You can inspect `Request.Headers` in the debugger
3. You see `HX-Request: true` in the headers (for htmx requests)

### 1.2 Network Requests: What "Good" Looks Like

Open browser DevTools → **Network** tab and perform these actions:

#### For `hx-post` Create (Form Submission)

| Check           | Expected Value                                     |
|-----------------|----------------------------------------------------|
| Request Method  | `POST`                                             |
| Request URL     | Contains `?handler=Create`                         |
| Request Headers | Includes `HX-Request: true`                        |
| Response Body   | HTML fragment starting with `<div id="task-list">` |
| Response Status | `200` (success and validation error)               |

**Example Request Headers:**

```
POST /Tasks?handler=Create HTTP/1.1
Host: localhost:5001
HX-Request: true
HX-Current-URL: https://localhost:5001/Tasks
Content-Type: application/x-www-form-urlencoded
```

#### For `hx-get` Refresh (List Refresh)

| Check           | Expected Value                                 |
|-----------------|------------------------------------------------|
| Request Method  | `GET`                                          |
| Request URL     | Contains `?handler=List`                       |
| Request Headers | Includes `HX-Request: true`                    |
| Response Body   | HTML fragment: `<div id="task-list">...</div>` |

#### For Validation Error Responses

| Check            | Expected Value                                                     |
|------------------|--------------------------------------------------------------------|
| Response Status  | `200` (Unprocessable Entity)                                       |
| Response Headers | `HX-Retarget: #task-form` and `HX-Reswap: outerHTML`               |
| Response Body    | HTML fragment: `<div id="task-form">...</div>` with error messages |

### 1.3 Inspect Returned Fragments in Elements Panel

Use the **Elements** tab in DevTools to verify swaps are working:

1. **Before submitting**: Note the DOM node for `div#task-list`
2. **After submitting**: The node should be replaced (different DOM reference)
3. **Confirm**: The content inside `#task-list` reflects the new data

**Key Insight:**

htmx swaps **HTML**, not JSON. Debugging htmx is about inspecting HTML fragments and target elements—not parsing API payloads.

---

## Part 2: Common Gotchas and Fixes

This section covers the five most common htmx + Razor Pages integration mistakes. For each, you'll learn the symptoms, likely causes, and fastest diagnostic steps.

### Gotcha A: Nothing Happens on Submit (or Button Click)

**Symptoms:**

- Clicking the button or submitting the form does nothing
- No network request appears in DevTools
- Page doesn't change at all

**Likely Causes:**

| Cause             | Why It Happens                                               |
|-------------------|--------------------------------------------------------------|
| htmx not loaded   | Script tag missing, blocked, or has error                    |
| Wrong file edited | Added `hx-*` to `Index.cshtml` instead of `_TaskForm.cshtml` |
| JavaScript error  | Console error prevented htmx from initializing               |

**Fast Diagnostic Steps:**

1. **Check Console for htmx:**

   Open DevTools Console and type:

   ```javascript
   htmx
   ```

   - If you see an object → htmx is loaded
   - If you see `undefined` → htmx is NOT loaded

2. **View Page Source:**

   Press `Ctrl+U` and search for `htmx`. Confirm the script tag exists:

   ```html
   <script src="https://unpkg.com/htmx.org@1.9.12"></script>
   ```

3. **Verify File Location:**

   Confirm you edited `Pages/Tasks/Partials/_TaskForm.cshtml`, not `Pages/Tasks/Index.cshtml`.

**Fix:**

```html
<!-- Add to _Layout.cshtml before </body> -->
<script src="https://unpkg.com/htmx.org@1.9.12"></script>
```

---

### Gotcha B: Request Fires, But Wrong Part Updates (or Nothing Updates)

**Symptoms:**

- Network tab shows the request completed successfully
- Response contains HTML
- But the page doesn't update, or the wrong element updates

**Likely Causes:**

| Cause                            | Why It Happens                                         |
|----------------------------------|--------------------------------------------------------|
| Wrong `hx-target` selector       | Typo in ID or selector doesn't match DOM               |
| Duplicate IDs                    | Multiple elements have the same ID                     |
| Response fragment shape mismatch | Using `outerHTML` but response doesn't include wrapper |

**Fast Diagnostic Steps:**

1. **Check for Single ID:**

   In Elements panel, search for `#task-list`. There should be exactly **one** element.

2. **Inspect Response Body:**

   In Network tab, click the request and view Response. Confirm it includes:

   ```html
   <div id="task-list">
       <!-- list content -->
   </div>
   ```

   The response must include the wrapper element when using `hx-swap="outerHTML"`.

3. **Verify Target Selector:**

   Check that `hx-target="#task-list"` matches the actual element ID (case-sensitive).

**Fix:**

Ensure your partial returns the complete wrapper:

```cshtml
@* _TaskList.cshtml - Must include the wrapper *@
<div id="task-list">
    @* Content here *@
</div>
```

**Common Mistake:**

```cshtml
@* WRONG - Missing wrapper, will break outerHTML swap *@
@foreach (var task in Model)
{
    <li>@task.Title</li>
}
```

---

### Gotcha C: Handler Not Found / Wrong Handler Invoked

**Symptoms:**

- Network tab shows `404 Not Found`
- Or: A different handler runs than expected
- Or: `OnGet` runs when you expected `OnPostCreate`

**Likely Causes:**

| Cause                 | Why It Happens                              |
|-----------------------|---------------------------------------------|
| Handler name mismatch | `?handler=Create` but method is `OnPostAdd` |
| Wrong HTTP verb       | `hx-get` calling an `OnPost...` handler     |
| Missing handler       | Method doesn't exist in PageModel           |

**Fast Diagnostic Steps:**

1. **Check Request URL:**

   In Network tab, verify the URL includes the correct handler:

   ```
   /Tasks?handler=Create    ← Should match OnPostCreate
   /Tasks?handler=List      ← Should match OnGetList
   ```

2. **Verify Method Names:**

   Razor Pages naming convention:

   | Handler Query                 | Method Signature       |
   |-------------------------------|------------------------|
   | `?handler=Create` (POST)      | `OnPostCreate()`       |
   | `?handler=List` (GET)         | `OnGetList()`          |
   | `?handler=Details&id=1` (GET) | `OnGetDetails(int id)` |

3. **Check HTTP Verb:**

   - `hx-post` → calls `OnPost...` handlers
   - `hx-get` → calls `OnGet...` handlers

**Fix:**

Ensure handler names align:

```html
<!-- Markup -->
<form hx-post="?handler=Create" ...>

<!-- Must match -->
public IActionResult OnPostCreate() { ... }
```

---

### Gotcha D: Partial Path Errors (Runtime View Not Found)

**Symptoms:**

- Exception message: "The partial view '...' was not found"
- Or: Wrong partial renders

**Likely Causes:**

| Cause                               | Why It Happens                           |
|-------------------------------------|------------------------------------------|
| Incorrect path in `Fragment()` call | Path doesn't match file location         |
| File in wrong folder                | Partial not where you think it is        |
| Typo in filename                    | `_TaskList.cshtml` vs `_Tasklist.cshtml` |

**Fast Diagnostic Steps:**

1. **Verify File Exists:**

   If your code says:

   ```csharp
   return Fragment("Partials/_TaskList", model);
   ```

   Then this file must exist:

   ```
   Pages/Tasks/Partials/_TaskList.cshtml
   ```

2. **Check Case Sensitivity:**

   On some systems, `_TaskList.cshtml` and `_Tasklist.cshtml` are different files.

3. **Use Consistent Paths:**

   Both of these should use the same path pattern:

   ```csharp
   // In handler
   return Fragment("Partials/_TaskList", Tasks);
   ```

   ```cshtml
   <!-- In Razor page -->
   <partial name="Partials/_TaskList" model="Model.Tasks" />
   ```

**Fix:**

Organize partials consistently:

```
Pages/Tasks/
├── Index.cshtml
├── Index.cshtml.cs
└── Partials/
    ├── _Messages.cshtml
    ├── _TaskForm.cshtml
    └── _TaskList.cshtml
```

---

### Gotcha E: Validation-as-You-Type Fails (403 Antiforgery or No Data Sent)

**Symptoms:**

- Keystroke validation returns `400 Bad Request` or `403 Forbidden`
- Request payload is empty or missing fields
- Server-side validation doesn't receive the input value

**Likely Causes:**

| Cause                     | Why It Happens                                 |
|---------------------------|------------------------------------------------|
| Antiforgery token missing | POST requests require the token                |
| Form fields not included  | `hx-post` on input doesn't send sibling fields |

**Fast Diagnostic Steps:**

1. **Check Request Payload:**

   In Network tab, check if the request includes `__RequestVerificationToken`:

   ```
   Input.Title=my+task&__RequestVerificationToken=CfDJ8...
   ```

2. **Verify Token in Form:**

   Ensure your form includes the antiforgery token:

   ```cshtml
   <form method="post" asp-page-handler="Create">
       @Html.AntiForgeryToken()
       <!-- or use: <input asp-antiforgery="true" /> -->
   ```

3. **Add `hx-include` if Needed:**

   If `hx-post` is on an input (not the form), add:

   ```html
   <input asp-for="Input.Title"
          hx-post="?handler=ValidateTitle"
          hx-include="closest form"
          ... />
   ```

**Fix:**

Ensure the form has the token and inputs include it:

```cshtml
<div id="task-form">
    <form method="post" asp-page-handler="Create"
          hx-post="?handler=Create"
          hx-target="#task-list"
          hx-swap="outerHTML">

        @Html.AntiForgeryToken()

        <input asp-for="Input.Title"
               hx-post="?handler=ValidateTitle"
               hx-include="closest form"
               hx-trigger="keyup changed delay:500ms"
               hx-target="#title-validation"
               hx-swap="outerHTML" />

        <!-- rest of form -->
    </form>
</div>
```

---

## Part 3: Standardize Response Conventions (Mini-Refactor)

Now that you can diagnose issues, let's establish conventions that prevent them.

### 3.1 The Response Rule Set

Every handler should return exactly one of these response types:

| Request Type | Response Type                  | When to Use          |
|--------------|--------------------------------|----------------------|
| Non-htmx     | `Page()` or `RedirectToPage()` | Full page navigation |
| htmx         | `Fragment("...", model)`       | Partial updates      |

**Rule 1: A handler returns exactly one of:**

```csharp
// Non-htmx: Full page
return Page();
return RedirectToPage();

// htmx: Fragment only
return Fragment("Partials/_TaskList", Tasks);
```

**Rule 2: Fragment handlers must return a wrapper with a stable ID:**

| Partial     | Must Return                     |
|-------------|---------------------------------|
| `_TaskList` | `<div id="task-list">...</div>` |
| `_TaskForm` | `<div id="task-form">...</div>` |
| `_Messages` | `<div id="messages">...</div>`  |

**Rule 3: Swap strategy must match fragment shape:**

| If You Use            | Response Must Be                        |
|-----------------------|-----------------------------------------|
| `hx-swap="outerHTML"` | Full wrapper element (`<div id="...">`) |
| `hx-swap="innerHTML"` | Content only (no wrapper needed)        |

### 3.2 Standardize Helper Methods

Ensure your PageModel includes these helpers:

**File: `Pages/Tasks/Index.cshtml.cs`**

```csharp
using Microsoft.AspNetCore.Mvc.ViewFeatures;

// Add inside your PageModel class:

/// <summary>
/// Checks if the current request was made by htmx.
/// htmx sends "HX-Request: true" header with every request.
/// </summary>
private bool IsHtmx() =>
    Request.Headers.TryGetValue("HX-Request", out var value) && value == "true";

/// <summary>
/// Returns a partial view result for fragment responses.
/// Preserves ViewData context for validation messages, etc.
/// </summary>
/// <param name="partialName">Path to the partial view</param>
/// <param name="model">Model to pass to the partial</param>
private PartialViewResult Fragment(string partialName, object model) =>
    new()
    {
        ViewName = partialName,
        ViewData = new ViewDataDictionary(ViewData) { Model = model }
    };
```

**Why These Helpers:**

- `IsHtmx()` removes magic strings scattered throughout handlers
- `Fragment()` ensures consistent `ViewData` propagation (critical for validation)
- Both are small enough to copy into any PageModel

### 3.3 Standardize Handler Naming

Adopt these naming conventions:

| Handler Name          | HTTP Verb | Returns               | Purpose               |
|-----------------------|-----------|-----------------------|-----------------------|
| `OnGet`               | GET       | Full Page             | Initial page load     |
| `OnGetList`           | GET       | `_TaskList`           | Refresh list fragment |
| `OnGetDetails`        | GET       | `_TaskDetails`        | Load details fragment |
| `OnGetMessages`       | GET       | `_Messages`           | Refresh messages      |
| `OnGetEmptyForm`      | GET       | `_TaskForm` (reset)   | Clear form            |
| `OnPostCreate`        | POST      | `_TaskList` (success) | Create new task       |
| `OnPostDelete`        | POST      | `_TaskList`           | Delete task           |
| `OnPostValidateTitle` | POST      | `_TitleValidation`    | Field validation      |

**The Pattern:**

- Handler name tells you which fragment it returns
- `OnGet*` for reads, `OnPost*` for mutations
- The `?handler=` value maps directly to the method suffix

### 3.4 Standardize Retarget Usage

**Policy: Use `HX-Retarget` + `HX-Reswap` only in two scenarios:**

| Scenario            | Original Target | Retarget To  | Why                          |
|---------------------|-----------------|--------------|------------------------------|
| Invalid form submit | `#task-list`    | `#task-form` | Show errors in place         |
| Server error        | `#task-list`    | `#messages`  | Route errors to message area |

**Everything else should use explicit `hx-target` in markup.**

**Example: Retargeting on Validation Error**

```csharp
if (!ModelState.IsValid)
{
    Tasks = InMemoryTaskStore.All();

    if (IsHtmx())
    {
        // Retarget from #task-list to #task-form
        Response.Headers["HX-Retarget"] = "#task-form";
        Response.Headers["HX-Reswap"] = "outerHTML";
        return Fragment("Partials/_TaskForm", this);
    }

    return Page();
}
```

---

## Part 4: Verification Exercise

Complete these checks before proceeding to Lab 3.

### Quick Verification Script (10 minutes)

**Step 1: Network Verification (2 minutes)**

1. Open DevTools → Network
2. Submit a valid task
3. Confirm:
   - Request includes `HX-Request: true`
   - Response is HTML fragment (not full page)
   - Only `#task-list` updates

**Step 2: Validation Error Verification (2 minutes)**

1. Submit an empty form
2. Confirm:
   - Response status is `200`
   - Response headers include `HX-Retarget: #task-form`
   - Form fragment replaces `#task-form` with error message

**Step 3: Refresh Button Verification (2 minutes)**

1. Click "Refresh All" button
2. Confirm:
   - GET request to `?handler=List`
   - Response is `#task-list` fragment
   - List updates without page reload

**Step 4: Helper Method Verification (4 minutes)**

1. Open `Index.cshtml.cs`
2. Verify `IsHtmx()` helper exists
3. Verify `Fragment()` helper exists
4. Verify all handlers follow the response rules

---

## "Ready to Proceed" Checklist

You are ready for Lab 3 when you can answer **"Yes"** to all of these:

- [ ] I can see htmx requests in Network tab with `HX-Request: true` header
- [ ] My Create updates only `#task-list` without a full page reload
- [ ] Invalid Create swaps the form fragment and shows validation errors
- [ ] Refresh button updates only `#task-list`
- [ ] I can locate the handler and partial returned for any interaction
- [ ] I have `IsHtmx()` and `Fragment()` helpers in my PageModel
- [ ] I understand when to use `HX-Retarget` (and when not to)

---

## Quick Reference: Diagnostic Commands

### Browser Console

```javascript
// Check if htmx is loaded
htmx                    // Should return an object

// Check htmx version
htmx.version            // e.g., "1.9.12"

// Manually trigger a request (debugging)
htmx.trigger(document.querySelector('#refresh-btn'), 'click')
```

### Network Tab Filters

- Filter by `XHR` to see only AJAX requests
- Search for `handler=` to find htmx requests
- Check `Response Headers` for `HX-*` headers

### Elements Tab

- Search for `#task-list` to find target element
- Watch for DOM node replacement during swaps
- Check for duplicate IDs (should be exactly one)

---

## Summary

This checkpoint covered:

1. **Development loop verification**: Ensure debugging and hot reload work
2. **Network inspection**: Understand what "good" htmx requests look like
3. **Common gotchas**: Diagnose and fix the five most frequent issues
4. **Response conventions**: Standardize how handlers respond to htmx vs. non-htmx requests
5. **Helper methods**: Establish `IsHtmx()` and `Fragment()` as standard patterns

With these foundations solid, you're ready for Lab 3: Real-Time Validation and Form UX.

---

## Troubleshooting Quick Reference

| Symptom                  | Likely Cause                      | Quick Fix                                 |
|--------------------------|-----------------------------------|-------------------------------------------|
| Nothing happens on click | htmx not loaded                   | Check console for `htmx` object           |
| Wrong element updates    | Wrong `hx-target` or duplicate ID | Verify single ID in Elements              |
| 404 on handler           | Name mismatch                     | Match `?handler=X` to `OnPost/GetX()`     |
| Partial not found        | Wrong path                        | Verify file exists at expected location   |
| 403 on POST              | Missing antiforgery token         | Add `@Html.AntiForgeryToken()`            |
| Nested duplicates        | Wrong swap mode                   | Use `outerHTML` with wrapper fragments    |
| Validation not showing   | Retarget missing                  | Add `HX-Retarget` and `HX-Reswap` headers |

---

## Appendix: Fragment Inventory Template

Add this comment to the top of your `Pages/Tasks/Index.cshtml`:

```cshtml
@*
    Fragment Inventory
    ==================

    | Target ID      | Partial                  | Handlers That Return It     |
    |----------------|--------------------------|------------------------------|
    | #messages      | Partials/_Messages       | OnGetMessages                |
    | #task-form     | Partials/_TaskForm       | OnGetEmptyForm, OnPostCreate (invalid) |
    | #task-list     | Partials/_TaskList       | OnGetList, OnPostCreate (success)      |
    | #task-details  | Partials/_TaskDetails    | OnGetDetails                 |

    Response Rules:
    - Non-htmx → Page() or RedirectToPage()
    - htmx → Fragment("...", model)
    - Validation error → HX-Retarget to form
    - Server error → 200 + HX-Retarget to messages
*@
```

This inventory prevents "where does this fragment come from?" confusion and documents your htmx contracts.

**Proceed to Lab 3: Real-Time Validation and Form UX →**
