---
order: 1
icon: code-square
---

# Checkpoint: Make It Production-Livable

## Overview

This checkpoint focuses on establishing maintainability patterns and conventions for htmx + Razor Pages applications. Before moving to more advanced patterns, it's essential to align on "how to structure this" so that your codebase remains clean, debuggable, and scalable.

Production-livable means your code is:

- **Discoverable**: Anyone can find the right file quickly
- **Predictable**: Patterns are consistent across features
- **Debuggable**: You can trace from request to response easily
- **Maintainable**: Adding new features follows established conventions

### Time Estimate

10–15 minutes

---

## Checkpoint Outcomes

By the end of this checkpoint, you will have:

| Outcome                | Description                                          |
|------------------------|------------------------------------------------------|
| **Folder conventions** | Clear structure for partials and handlers            |
| **Handler naming**     | Predictable, discoverable method names               |
| **Response rules**     | Consistent patterns for fragments vs pages vs errors |
| **Reusable helpers**   | Standard utilities you can copy to real projects     |
| **Fragment inventory** | Documentation of your htmx contracts                 |

---

## Part 1: Folder Conventions for Partials and Handlers (5–7 minutes)

A well-organized folder structure makes it easy to find and maintain code. The key principle: **co-locate fragments with the features that use them**.

### 1.1 Recommended Production Structure

```
Pages/
├── Tasks/
│   ├── Index.cshtml              # Page shell + composition
│   ├── Index.cshtml.cs           # PageModel, handlers, orchestration
│   └── Partials/                 # Fragment boundaries for this feature
│       ├── _TaskList.cshtml      # Fragment: #task-list
│       ├── _TaskForm.cshtml      # Fragment: #task-form
│       ├── _Messages.cshtml      # Fragment: #messages
│       ├── _TaskDetails.cshtml   # Fragment: #task-details
│       ├── _TitleValidation.cshtml # Field fragment: #title-validation
│       └── _Error.cshtml         # Dedicated error fragment

Models/
├── TaskItem.cs                   # Domain model
└── TaskListVm.cs                 # View model for list + pagination

Data/
└── InMemoryTaskStore.cs          # Data access (workshop version)
```

### 1.2 Why This Structure Works

| Aspect              | Benefit                                               |
|---------------------|-------------------------------------------------------|
| **Co-location**     | Partials live next to the page that owns them         |
| **Discoverability** | File paths mirror the mental model                    |
| **Ownership**       | Fragment boundaries are "owned" by a specific feature |
| **Scalability**     | Easy to add new features following the same pattern   |

### 1.3 Understanding Fragment Boundaries

Each partial represents a **fragment boundary**—a swappable region of the UI:

| Partial                   | Fragment ID           | Purpose                             |
|---------------------------|-----------------------|-------------------------------------|
| `_TaskList.cshtml`        | `#task-list`          | List of tasks (paginated, filtered) |
| `_TaskForm.cshtml`        | `#task-form`          | Create/edit form                    |
| `_Messages.cshtml`        | `#messages`           | Success/error messages              |
| `_TaskDetails.cshtml`     | `#task-details`       | Detail panel/modal content          |
| `_TitleValidation.cshtml` | `#title-validation`   | Field-level validation              |
| `_Error.cshtml`           | (targets `#messages`) | Error display                       |

### 1.4 Handler Placement Rule

**Keep htmx handlers on the same PageModel when:**

- They return fragments used only by that page (most common case)
- The fragments are feature-specific

**Promote to dedicated endpoints when:**

- Multiple pages share the same fragment endpoint (true reuse)
- The fragment becomes an independent "resource" with its own lifecycle
- You need different authorization or caching rules

**Example: When to Extract**

```csharp
// Keep in TasksIndexModel - used only by Tasks page
public IActionResult OnGetList(string? q, int page = 1) { ... }

// Consider extracting to Minimal API - used by multiple features
// GET /api/notifications (used by Tasks, Dashboard, Header)
app.MapGet("/api/notifications", () => { ... });
```

### 1.5 Verify Your Structure

**Checklist:**

- [ ] Partials folder exists under each feature folder
- [ ] Each partial has a clear, single responsibility
- [ ] Partial file names start with underscore (`_`)
- [ ] Fragment IDs are documented (even as comments)

---

## Part 2: Consistent Handler Naming (5–7 minutes)

Handler names should be **self-documenting**. Anyone reading the code should know what a handler returns just from its name.

### 2.1 Handler Naming Convention

Use **resource + intent** naming aligned with HTTP verbs:

#### GET Fragment Handlers

| Handler                | Returns             | Purpose                    |
|------------------------|---------------------|----------------------------|
| `OnGetList(...)`       | `_TaskList`         | Fetch the list fragment    |
| `OnGetDetails(int id)` | `_TaskDetails`      | Fetch details for one item |
| `OnGetMessages()`      | `_Messages`         | Fetch messages fragment    |
| `OnGetEmptyForm()`     | `_TaskForm` (reset) | Fetch a clean form         |

#### POST Action Handlers

| Handler                 | Returns                       | Purpose                 |
|-------------------------|-------------------------------|-------------------------|
| `OnPostCreate()`        | `_TaskList` (success)         | Create new item         |
| `OnPostDelete(int id)`  | `_TaskList`                   | Delete an item          |
| `OnPostValidateTitle()` | `_TitleValidation`            | Validate a single field |
| `OnPostUpdate(int id)`  | `_TaskList` or `_TaskDetails` | Update an item          |

### 2.2 The Naming Rule

**If the handler returns a fragment, its name should tell you which fragment it serves.**

```csharp
// GOOD: Name tells you what it returns
public IActionResult OnGetList(...) { }      // Returns _TaskList
public IActionResult OnGetDetails(int id) { } // Returns _TaskDetails
public IActionResult OnPostValidateTitle() { } // Returns _TitleValidation

// BAD: Ambiguous names
public IActionResult OnGetData(...) { }       // What data?
public IActionResult OnPostSubmit() { }       // Submit what?
public IActionResult OnGetFragment() { }      // Which fragment?
```

### 2.3 URL Convention in Markup

Keep URLs consistent and predictable:

```html
<!-- GET handlers -->
hx-get="?handler=List"           ↔ OnGetList()
hx-get="?handler=Details&id=123" ↔ OnGetDetails(int id)
hx-get="?handler=Messages"       ↔ OnGetMessages()
hx-get="?handler=EmptyForm"      ↔ OnGetEmptyForm()

<!-- POST handlers -->
hx-post="?handler=Create"        ↔ OnPostCreate()
hx-post="?handler=Delete"        ↔ OnPostDelete(int id)
hx-post="?handler=ValidateTitle" ↔ OnPostValidateTitle()
```

### 2.4 Why This Matters for Debugging

With consistent naming, debugging becomes trivial:

1. **Open Network tab** in DevTools
2. **See request URL**: `?handler=Details&id=42`
3. **Know immediately**: Look in `OnGetDetails(int id)`
4. **Find the partial**: Handler returns `_TaskDetails`

No guessing, no searching—the URL maps directly to a method name.

### 2.5 Verify Your Naming

**Checklist:**

- [ ] Handler names follow verb + resource pattern
- [ ] GET handlers start with `OnGet`
- [ ] POST handlers start with `OnPost`
- [ ] Names indicate which fragment they return
- [ ] URL patterns in markup match handler names exactly

---

## Part 3: Basic Response Rules (5–7 minutes)

These rules prevent "htmx spaghetti"—random swaps, inconsistent fragments, and mixed concerns.

### 3.1 Response Rule 1: Full Navigation Returns Pages

**When**: The interaction is classic navigation (non-htmx request)

**Return**: `Page()` or `RedirectToPage()`

**Examples:**

```csharp
// Initial page load
public void OnGet()
{
    Tasks = InMemoryTaskStore.All();
    // Implicit return Page()
}

// Non-htmx form submit (fallback)
public IActionResult OnPostCreate()
{
    // ... validation and save ...

    if (!IsHtmx())
    {
        FlashMessage = "Task added.";
        return RedirectToPage(); // Full page redirect
    }

    // htmx path below...
}
```

### 3.2 Response Rule 2: Fragment Interactions Return Partials

**When**: The request is htmx (`HX-Request: true`)

**Return**: A `PartialViewResult` containing only the fragment

**Examples:**

```csharp
// List fragment
public IActionResult OnGetList(string? q, int page = 1, int pageSize = 5)
{
    // ... build view model ...
    return Fragment("Partials/_TaskList", vm);
}

// Details fragment
public IActionResult OnGetDetails(int id)
{
    var task = InMemoryTaskStore.Find(id);
    return Fragment("Partials/_TaskDetails", task);
}

// Success path in OnPostCreate
if (IsHtmx())
{
    FlashMessage = "Task added successfully!";
    Response.Headers["HX-Trigger"] = "showMessage,clearForm";
    return Fragment("Partials/_TaskList", vm);
}
```

### 3.3 The Fragment Contract

**Critical Rule**: If you use `hx-swap="outerHTML"`, the fragment **must** include its wrapper with the correct ID.

| Fragment       | Must Return                        |
|----------------|------------------------------------|
| `_TaskList`    | `<div id="task-list">...</div>`    |
| `_TaskForm`    | `<div id="task-form">...</div>`    |
| `_Messages`    | `<div id="messages">...</div>`     |
| `_TaskDetails` | `<div id="task-details">...</div>` |

**Why This Matters:**

```html
<!-- Markup expects to swap #task-list -->
<button hx-get="?handler=List"
        hx-target="#task-list"
        hx-swap="outerHTML">

<!-- Fragment MUST include the wrapper -->
<div id="task-list">
    <!-- list content -->
</div>
```

If the fragment doesn't include the wrapper, htmx won't find the element to swap.

### 3.4 Response Rule 3: Errors Return Dedicated Fragments

Have a consistent error handling strategy:

#### Validation Errors

```csharp
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
```

#### Not Found Errors

```csharp
if (!removed)
{
    if (IsHtmx())
    {
        Response.Headers["HX-Retarget"] = "#messages";
        Response.Headers["HX-Reswap"] = "outerHTML";
        return Fragment("Partials/_Messages", "Task not found.");
    }

    FlashMessage = "Task not found.";
    return RedirectToPage();
}
```

#### Server Errors

```csharp
if (IsHtmx())
{
    Response.Headers["HX-Retarget"] = "#messages";
    Response.Headers["HX-Reswap"] = "innerHTML";
    return Fragment("Partials/_Error", "An unexpected error occurred.");
}

throw new InvalidOperationException("Server error.");
```

### 3.5 Response Rules Summary Table

| Scenario              | Status | Response                      | Target                   |
|-----------------------|--------|-------------------------------|--------------------------|
| Initial page load     | 200    | `Page()`                      | Full page                |
| Non-htmx form submit  | 302    | `RedirectToPage()`            | Full page                |
| htmx success          | 200    | `Fragment("...", model)`      | Original `hx-target`     |
| htmx validation error | 200    | `Fragment("_TaskForm", this)` | Retarget to `#task-form` |
| htmx not found        | 200    | `Fragment("_Messages", msg)`  | Retarget to `#messages`  |
| htmx server error     | 200    | `Fragment("_Error", msg)`     | Retarget to `#messages`  |

### 3.6 When to Use Retargeting

**Use `HX-Retarget` + `HX-Reswap` only in these cases:**

1. **Invalid submit**: Form targets the list, but on validation failure you need to update the form instead
2. **Error routing**: Route errors to `#messages` from any interaction

**Everything else** should use explicit `hx-target` in markup.

**Why limit retargeting?**

- Keeps behavior predictable
- Makes debugging easier (what you see in markup is what happens)
- Prevents "action at a distance" bugs

---

## Part 4: Standardize PageModel Helpers (3–5 minutes)

Every PageModel that serves htmx requests should have these helpers:

### 4.1 The IsHtmx() Helper

```csharp
/// <summary>
/// Checks if the current request was made by htmx.
/// </summary>
private bool IsHtmx() =>
    Request.Headers.TryGetValue("HX-Request", out var value) && value == "true";
```

**Why it matters:**

- Removes magic strings from handler code
- Makes branching explicit and readable
- Easy to test (can mock header in tests)

### 4.2 The Fragment() Helper

```csharp
/// <summary>
/// Returns a partial view result for fragment responses.
/// Properly propagates ViewData (including ModelState) to the partial.
/// </summary>
private PartialViewResult Fragment(string partialName, object model) =>
    new()
    {
        ViewName = partialName,
        ViewData = new ViewDataDictionary(ViewData) { Model = model }
    };
```

**Why it matters:**

- Ensures ViewData (including ModelState) flows to partials
- Consistent return type across all fragment handlers
- Cleaner than repeating `new PartialViewResult { ... }` everywhere

### 4.3 Required Using Statement

Add this to your PageModel:

```csharp
using Microsoft.AspNetCore.Mvc.ViewFeatures;
```

### 4.4 Complete Helper Section

Here's how the helpers look in context:

```csharp
public class IndexModel : PageModel
{
    // Properties...

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

    // Handlers...
}
```

### 4.5 Verify Your Helpers

**Checklist:**

- [ ] `IsHtmx()` helper exists in PageModel
- [ ] `Fragment()` helper exists in PageModel
- [ ] `using Microsoft.AspNetCore.Mvc.ViewFeatures;` is added
- [ ] Handlers use `IsHtmx()` instead of checking headers directly
- [ ] Handlers use `Fragment()` instead of constructing PartialViewResult manually

---

## Part 5: Fragment Inventory (3–5 minutes)

Document your fragment contracts to prevent accidental ID drift and broken swaps.

### 5.1 Create a Fragment Inventory

Add a comment block to your page's Razor file or PageModel:

**Option A: In Index.cshtml**

```cshtml
@*
    Fragment Inventory for Tasks Page
    ==================================

    Fragment ID          | Partial              | Swap Strategy | Returned By
    ---------------------|----------------------|---------------|------------------
    #task-list           | _TaskList            | outerHTML     | OnGetList, OnPostCreate, OnPostDelete
    #task-form           | _TaskForm            | outerHTML     | OnGetEmptyForm, OnPostCreate (invalid)
    #messages            | _Messages            | outerHTML     | OnGetMessages, error handlers
    #task-details        | _TaskDetails         | outerHTML     | OnGetDetails
    #title-validation    | _TitleValidation     | outerHTML     | OnPostValidateTitle

    Response Rules:
    - Success responses target original hx-target
    - Validation errors retarget to #task-form
    - Not found and server errors retarget to #messages
*@
```

**Option B: In Index.cshtml.cs**

```csharp
/// <summary>
/// Tasks page with htmx fragment support.
///
/// Fragment Inventory:
/// - #task-list → _TaskList (OnGetList, OnPostCreate, OnPostDelete)
/// - #task-form → _TaskForm (OnGetEmptyForm, OnPostCreate invalid)
/// - #messages → _Messages (OnGetMessages, error handlers)
/// - #task-details → _TaskDetails (OnGetDetails)
/// - #title-validation → _TitleValidation (OnPostValidateTitle)
/// </summary>
public class IndexModel : PageModel
{
    // ...
}
```

### 5.2 Why Document Fragment Contracts?

| Benefit               | Description                                    |
|-----------------------|------------------------------------------------|
| **Prevents ID drift** | Team knows which IDs are "reserved"            |
| **Debugging aid**     | Quick reference for which handler returns what |
| **Onboarding**        | New developers understand the page structure   |
| **Maintenance**       | Safely rename/refactor with full visibility    |

### 5.3 URL State Policy (If Using Filtering/Pagination)

Document your URL state policy:

```cshtml
@*
    URL State Policy:
    - List filtering uses hx-push-url="true"
    - Query parameters: q (filter), page (pagination), pageSize
    - Initial OnGet() must accept same parameters as OnGetList()
    - Back/forward navigation should restore filter/page state
*@
```

---

## Part 6: Quick Reference Card

### Response Decision Tree

```
Is this an htmx request? (HX-Request: true)
├── NO → Return Page() or RedirectToPage()
└── YES
    ├── Success? → Return Fragment() to original hx-target
    ├── Validation error? → Retarget to form fragment
    └── Server error? → Retarget to messages
```

### Handler Naming Quick Reference

| Pattern                 | Example                 | Returns               |
|-------------------------|-------------------------|-----------------------|
| `OnGet{Resource}`       | `OnGetList()`           | `_TaskList`           |
| `OnGet{Resource}`       | `OnGetDetails(int id)`  | `_TaskDetails`        |
| `OnPost{Action}`        | `OnPostCreate()`        | `_TaskList` (success) |
| `OnPost{Action}`        | `OnPostDelete(int id)`  | `_TaskList`           |
| `OnPostValidate{Field}` | `OnPostValidateTitle()` | `_TitleValidation`    |

### Status Codes Quick Reference

| Status | Meaning  | Use Case             |
|--------|----------|----------------------|
| 200    | OK       | Successful response  |
| 302    | Redirect | Non-htmx form submit |

### htmx Headers Quick Reference

| Header        | Direction | Purpose                 |
|---------------|-----------|-------------------------|
| `HX-Request`  | Request   | Identifies htmx request |
| `HX-Trigger`  | Response  | Fire client-side events |
| `HX-Retarget` | Response  | Override hx-target      |
| `HX-Reswap`   | Response  | Override hx-swap        |
| `HX-Push-Url` | Response  | Update browser URL      |

---

## Verification Checklist

Before moving to Lab 5, verify these items:

### Folder Structure

- [ ] Partials folder exists under `Pages/Tasks/`
- [ ] Each fragment has a dedicated partial file
- [ ] Partial names start with underscore

### Handler Naming

- [ ] GET handlers follow `OnGet{Resource}` pattern
- [ ] POST handlers follow `OnPost{Action}` pattern
- [ ] Handler names indicate which fragment they return

### Response Rules

- [ ] Non-htmx requests return `Page()` or `RedirectToPage()`
- [ ] htmx requests return `Fragment()`
- [ ] Error responses retarget to `#messages`

### Helpers

- [ ] `IsHtmx()` helper is implemented
- [ ] `Fragment()` helper is implemented
- [ ] Handlers use helpers consistently

### Documentation

- [ ] Fragment inventory exists (comment or doc)
- [ ] Fragment IDs are documented
- [ ] Handler-to-fragment mapping is clear

---

## "Ready to Proceed" Gate

Attendees should be able to answer "yes" to all of these:

1. **I can locate fragments**: I know exactly where to find `_TaskList.cshtml` and other partials
2. **I can trace requests**: Given a URL like `?handler=Details&id=42`, I can find the handler immediately
3. **I understand response rules**: I know when to return `Page()` vs `Fragment()` vs retarget
4. **I have consistent helpers**: `IsHtmx()` and `Fragment()` are in my PageModel
5. **I can debug swaps**: I can verify fragment boundaries in DevTools

---

## Key Takeaways

### The Core Principle

**htmx + Razor Pages is about discipline, not complexity.**

The patterns are simple:

- Fragment boundaries with stable IDs
- Handlers that return partials
- Consistent naming and response rules

The discipline is maintaining these patterns consistently across your codebase.

### What Makes Code "Production-Livable"

| Attribute        | How to Achieve It                                  |
|------------------|----------------------------------------------------|
| **Discoverable** | Co-locate partials with features, use clear naming |
| **Predictable**  | Follow response rules, name handlers consistently  |
| **Debuggable**   | URL matches handler, fragment matches ID           |
| **Maintainable** | Document contracts, use helpers                    |

### The Anti-Patterns to Avoid

| Anti-Pattern           | Why It's Bad           | Better Approach              |
|------------------------|------------------------|------------------------------|
| Random hx-targets      | Hard to debug          | Explicit fragment boundaries |
| Inconsistent naming    | Can't find handlers    | Verb + resource pattern      |
| Mixed response types   | Unpredictable behavior | Clear response rules         |
| Magic strings          | Hard to refactor       | Use helpers                  |
| Undocumented fragments | ID drift, broken swaps | Fragment inventory           |

---

## What Comes Next

In **Lab 5**, you'll implement:

- Dynamic form rows (Add/Remove tag inputs)
- Dependent dropdowns (Category → Subcategory)
- Long-running operations with polling
- Out-of-band swaps for global updates

With your codebase now "production-livable," these advanced patterns will be easier to implement and maintain.

**Proceed to Lab 5: Dynamic Forms + Long-Running UX (Polling) →**
