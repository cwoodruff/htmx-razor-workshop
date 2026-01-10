---
order: 1
icon: code-square
---

# Lab 1: Baseline Razor Pages App + "Fragment First"

## Overview

Welcome to the first lab of the htmx + ASP.NET Core Workshop! In this lab, you will build the foundation that all subsequent labs depend upon. By the end of this lab, you will have a fully functional Razor Pages application with a clean "Fragment First" architecture—a design pattern that makes htmx integration seamless in later labs.

### What is "Fragment First"?

"Fragment First" is an architectural approach where you design your UI as composable HTML fragments from the very beginning, *before* adding any htmx interactivity. Each fragment:

- Has a **single responsibility** (display a list, render a form, show messages)
- Lives in its own **partial view file**
- Contains a **stable root element with a predictable ID** (e.g., `#task-list`, `#task-form`)

This approach treats partials not as "optimization" or "code organization," but as **interfaces for future interactivity**. When you later add htmx, each fragment becomes a swappable unit that the server can return independently.

### Why This Matters

Traditional server-rendered applications return full pages on every interaction. With htmx, you can return *just the fragment that changed*. But this only works well if your fragments are designed with clear boundaries from the start. The "Fragment First" pattern ensures:

1. **Predictable targets**: Every region has a known ID that htmx can target
2. **Clean separation**: Each partial handles one concern
3. **Easy testing**: You can render and verify fragments in isolation
4. **Progressive enhancement**: The app works without htmx; adding it later is additive

---

## Lab Outcomes

By the end of this lab, you will have:

| Outcome | Description |
|---------|-------------|
| **Working Razor Pages app** | An application you can run and debug locally |
| **Simple domain model** | A "Tasks" domain with in-memory storage |
| **Three fragment boundaries** | List region, form region, and message region |
| **Three partial views** | `_TaskList.cshtml`, `_TaskForm.cshtml`, `_Messages.cshtml` |
| **Stable target element IDs** | `#task-list`, `#task-form`, `#messages` |

This becomes the "frame" for all htmx work in subsequent labs.

---

## Prerequisites

Before starting this lab, ensure you have:

- **.NET 8 SDK or later** installed ([download](https://dotnet.microsoft.com/download))
- **An IDE or editor**: Visual Studio 2022, JetBrains Rider, or VS Code with C# extensions
- **A modern web browser** with developer tools (Chrome, Edge, or Firefox)
- **Basic familiarity** with C#, Razor Pages, and HTML

---

## Step 1: Create a New Razor Pages Project (5–8 minutes)

We will create the project from scratch using the .NET CLI. This gives you full control over the project structure and ensures everyone starts from the same baseline.

### 1.1 Open Your Terminal

Open a terminal or command prompt and navigate to your preferred working directory.

### 1.2 Create the Project

Run the following commands to create a new Razor Pages web application:
````bash
dotnet new webapp -n RazorHtmxWorkshop
cd RazorHtmxWorkshop
```

**What this does:**

- `dotnet new webapp` creates a new ASP.NET Core Razor Pages application
- `-n RazorHtmxWorkshop` names the project "RazorHtmxWorkshop"
- `cd RazorHtmxWorkshop` navigates into the project directory

### 1.3 Verify the Project Structure

Your project should have this structure:
```
RazorHtmxWorkshop/
├── Pages/
│   ├── Shared/
│   │   ├── _Layout.cshtml
│   │   └── _ValidationScriptsPartial.cshtml
│   ├── _ViewImports.cshtml
│   ├── _ViewStart.cshtml
│   ├── Error.cshtml
│   ├── Index.cshtml
│   └── Privacy.cshtml
├── Properties/
│   └── launchSettings.json
├── wwwroot/
│   ├── css/
│   └── lib/
├── appsettings.json
├── appsettings.Development.json
├── Program.cs
└── RazorHtmxWorkshop.csproj
````

### 1.4 Run the Application

Start the application to verify everything works:
````bash
dotnet run
```

You should see output similar to:
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5000
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: https://localhost:5001
````

### 1.5 Open in Browser

Open your browser and navigate to the URL shown (e.g., `https://localhost:5001`). You should see the default ASP.NET Core welcome page.

**Keep the browser open**—you will refresh frequently throughout this lab.

> **Tip**: If you prefer, you can use `dotnet watch run` instead of `dotnet run`. This enables hot reload, automatically rebuilding and refreshing when you save changes.

---

## Step 2: Create the Domain Model (10–12 minutes)

Now we will establish a simple domain: **Tasks**. We are intentionally keeping this minimal so we can focus on UI patterns rather than data complexity.

### Design Constraints

For this workshop, we follow these constraints:

- **Single page focus**: One page that will become highly interactive with htmx
- **In-memory storage**: No database setup required; focus stays on UI patterns
- **Fully server-rendered**: No htmx yet—that comes in Lab 2

### 2.1 Create the TaskItem Model

Create a new folder called `Models` in your project root, then create a file named `TaskItem.cs`:

**File: `Models/TaskItem.cs`**
````csharp
namespace RazorHtmxWorkshop.Models;

/// <summary>
/// Represents a single task in our task management domain.
/// This is intentionally simple—we want to focus on UI patterns, not domain complexity.
/// </summary>
public class TaskItem
{
    /// <summary>
    /// Unique identifier for the task.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// The task's title/description.
    /// </summary>
    public string Title { get; set; } = "";

    /// <summary>
    /// Whether the task has been completed.
    /// </summary>
    public bool IsDone { get; set; }

    /// <summary>
    /// When the task was created (UTC).
    /// </summary>
    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
}
````

**Why this design:**

- `Id`: We need a way to identify individual tasks for future operations (edit, delete)
- `Title`: The primary content users will see and interact with
- `IsDone`: A simple status flag for visual differentiation
- `CreatedUtc`: Useful for sorting (newest first) and displaying creation time

### 2.2 Create the In-Memory Task Store

Create a new folder called `Data` in your project root, then create a file named `InMemoryTaskStore.cs`:

**File: `Data/InMemoryTaskStore.cs`**
````csharp
using RazorHtmxWorkshop.Models;

namespace RazorHtmxWorkshop.Data;

/// <summary>
/// A simple in-memory store for TaskItems.
///
/// Design notes:
/// - Static class for simplicity in a workshop context
/// - In production, you would use Entity Framework Core, Dapper, or another data access approach
/// - Data is lost when the application restarts—this is intentional for workshop isolation
/// </summary>
public static class InMemoryTaskStore
{
    // Auto-incrementing ID counter
    private static int _nextId = 1;

    // The actual storage - a simple list
    private static readonly List<TaskItem> _tasks = new();

    /// <summary>
    /// Returns all tasks, ordered by creation date (newest first).
    /// Returns a new list to prevent external modification of internal state.
    /// </summary>
    public static IReadOnlyList<TaskItem> All() => _tasks
        .OrderByDescending(t => t.CreatedUtc)
        .ToList();

    /// <summary>
    /// Adds a new task with the given title.
    /// Automatically assigns an ID and creation timestamp.
    /// </summary>
    /// <param name="title">The task title (will be trimmed)</param>
    /// <returns>The newly created TaskItem</returns>
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
}
```

**Why an in-memory store:**

- **Zero setup**: No database connection strings, migrations, or external dependencies
- **Fast iteration**: Changes are immediate; no waiting for database operations
- **Workshop isolation**: Each attendee's data is independent; restarting clears everything
- **Pattern focus**: We can concentrate on htmx and Razor Pages patterns, not data access

> **Production Note**: In a real application, you would replace this with a proper data access layer using Entity Framework Core, Dapper, or your preferred ORM. The interface (methods like `All()` and `Add()`) would remain similar.

---

## Step 3: Create the Tasks Page (10–12 minutes)

Now we will create the main Tasks page with its PageModel. This page will host all three fragment regions.

### 3.1 Create the Tasks Folder Structure

Create the following folder structure:
```
Pages/
└── Tasks/
    ├── Index.cshtml
    └── Index.cshtml.cs
````

You can create these files manually or use your IDE's scaffolding features.

### 3.2 Create the PageModel

**File: `Pages/Tasks/Index.cshtml.cs`**
````csharp
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RazorHtmxWorkshop.Data;
using RazorHtmxWorkshop.Models;

namespace RazorHtmxWorkshop.Pages.Tasks;

/// <summary>
/// PageModel for the Tasks index page.
///
/// This handles:
/// - Displaying the list of tasks (OnGet)
/// - Creating new tasks (OnPostCreate)
///
/// Design notes:
/// - Tasks property is populated on every request that needs to display the list
/// - Input property uses [BindProperty] for model binding on POST
/// - FlashMessage uses [TempData] to survive redirects
/// </summary>
public class IndexModel : PageModel
{
    /// <summary>
    /// The list of tasks to display. Populated in handlers that render the page.
    /// </summary>
    public IReadOnlyList<TaskItem> Tasks { get; private set; } = Array.Empty<TaskItem>();

    /// <summary>
    /// Input model for creating new tasks. Bound automatically on POST requests.
    /// </summary>
    [BindProperty]
    public NewTaskInput Input { get; set; } = new();

    /// <summary>
    /// Flash message to display after redirects.
    /// TempData survives one redirect, making it perfect for "success" messages.
    /// </summary>
    [TempData]
    public string? FlashMessage { get; set; }

    /// <summary>
    /// Handles GET requests to /Tasks.
    /// Loads all tasks for display.
    /// </summary>
    public void OnGet()
    {
        Tasks = InMemoryTaskStore.All();
    }

    /// <summary>
    /// Handles POST requests to /Tasks?handler=Create.
    /// Validates input, creates the task, and redirects back to the page.
    ///
    /// The Post-Redirect-Get (PRG) pattern prevents duplicate submissions
    /// when users refresh the page after submitting.
    /// </summary>
    public IActionResult OnPostCreate()
    {
        // Manual validation (we'll add data annotations in Lab 3)
        if (string.IsNullOrWhiteSpace(Input.Title))
        {
            ModelState.AddModelError(nameof(Input.Title), "Title is required.");
        }

        // If validation failed, redisplay the page with errors
        if (!ModelState.IsValid)
        {
            Tasks = InMemoryTaskStore.All();
            return Page();
        }

        // Create the task
        InMemoryTaskStore.Add(Input.Title);

        // Set success message and redirect (PRG pattern)
        FlashMessage = "Task added.";
        return RedirectToPage();
    }

    /// <summary>
    /// Input model for the create task form.
    /// Nested class keeps it close to its usage context.
    /// </summary>
    public class NewTaskInput
    {
        public string Title { get; set; } = "";
    }
}
````

**Key concepts explained:**

| Concept | Purpose |
|---------|---------|
| `[BindProperty]` | Automatically populates `Input` from form data on POST |
| `[TempData]` | Stores data that survives exactly one redirect |
| `OnGet()` | Handles HTTP GET requests |
| `OnPostCreate()` | Handles HTTP POST to `?handler=Create` |
| PRG Pattern | Post-Redirect-Get prevents duplicate form submissions |

### 3.3 Create the Razor Page (Initial Version)

**File: `Pages/Tasks/Index.cshtml`**
````cshtml
@page
@model RazorHtmxWorkshop.Pages.Tasks.IndexModel
@{
    ViewData["Title"] = "Tasks";
}

<h1>Tasks</h1>

@* Messages region - will become #messages fragment *@
<div id="messages">
    @if (!string.IsNullOrWhiteSpace(Model.FlashMessage))
    {
        <div class="alert alert-success" role="alert">
            @Model.FlashMessage
        </div>
    }
</div>

<div class="row">
    @* Form region - will become #task-form fragment *@
    <div class="col-md-5">
        <h2 class="h5">Add a Task</h2>
        <div id="task-form">
            <form method="post" asp-page-handler="Create">
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
    </div>

    @* List region - will become #task-list fragment *@
    <div class="col-md-7">
        <h2 class="h5">Current Tasks</h2>
        <div id="task-list">
            @if (Model.Tasks.Count == 0)
            {
                <p class="text-muted">No tasks yet.</p>
            }
            else
            {
                <ul class="list-group">
                    @foreach (var task in Model.Tasks)
                    {
                        <li class="list-group-item d-flex justify-content-between align-items-center">
                            <span>@task.Title</span>
                            @if (task.IsDone)
                            {
                                <span class="badge bg-success">Done</span>
                            }
                        </li>
                    }
                </ul>
            }
        </div>
    </div>
</div>

@section Scripts {
    <partial name="_ValidationScriptsPartial" />
}
```

### 3.4 Test the Basic Functionality

1. **Stop** any running instance of the application
2. **Run** the application: `dotnet run`
3. **Navigate** to `/Tasks` in your browser
4. **Test** the following:
   - The page loads with "No tasks yet." message
   - Enter a task title and click "Add Task"
   - The page reloads with your task in the list
   - The "Task added." success message appears
   - Try submitting an empty form—validation error should appear

At this point, you have a fully functional (but traditional) server-rendered page. Every interaction causes a full page reload. In the next steps, we will refactor this into the "Fragment First" architecture.

---

## Step 4: Identify Fragment Boundaries (5 minutes)

Before creating partials, we need to clearly identify which parts of the UI should become independent fragments.

### The Fragment Boundary Rule

> Any part of the UI that might update independently becomes a fragment boundary.

When htmx is added later, it will request and replace *only that fragment*—not the entire page.

### Fragment Boundaries for Our Tasks Page

Looking at our page, we identify **three** fragment boundaries:

| Fragment | Element ID | Purpose | When It Updates |
|----------|------------|---------|-----------------|
| **Messages** | `#messages` | Success/error notifications | After create, delete, or error |
| **Form** | `#task-form` | Create task form | After validation errors, form reset |
| **List** | `#task-list` | Display all tasks | After create, delete, refresh |

### Visual Representation
```
┌─────────────────────────────────────────────────────────────┐
│  Tasks Page                                                  │
├─────────────────────────────────────────────────────────────┤
│  ┌─────────────────────────────────────────────────────────┐ │
│  │  #messages - Success/error messages                     │ │
│  └─────────────────────────────────────────────────────────┘ │
│                                                              │
│  ┌─────────────────────┐  ┌─────────────────────────────────┐ │
│  │  #task-form         │  │  #task-list                    │ │
│  │                     │  │                                 │ │
│  │  - Title input      │  │  - Task 1                       │ │
│  │  - Submit button    │  │  - Task 2                       │ │
│  │                     │  │  - Task 3                       │ │
│  └─────────────────────┘  └─────────────────────────────────┘ │
└─────────────────────────────────────────────────────────────┘
```

### Important Design Consideration

Fragment boundaries are about **product UX**, not technical layering. Ask yourself:

- "What does the user expect to change when they perform this action?"
- "What should stay the same?"

For example, when a user creates a task:
- The **list** should update (show the new task)
- The **messages** should update (show "Task added")
- The **form** might reset (clear the input)

Each of these is a separate concern, so each becomes a fragment.

---

## Step 5: Create the Partial Views (10–12 minutes)

Now we extract each fragment region into its own partial view. This is the core of the "Fragment First" pattern.

### 5.1 Create the Partials Folder

Create a new folder structure:
```
Pages/
└── Tasks/
    ├── Partials/
    │   ├── _Messages.cshtml
    │   ├── _TaskForm.cshtml
    │   └── _TaskList.cshtml
    ├── Index.cshtml
    └── Index.cshtml.cs
````

### 5.2 Create the Messages Partial

**File: `Pages/Tasks/Partials/_Messages.cshtml`**
````cshtml
@model string?

@*
    Messages Fragment
    =================

    Purpose:
    - Displays success/error messages to the user
    - Wraps content in a stable #messages container

    Design notes:
    - The outer <div id="messages"> MUST always render, even when empty
    - This ensures htmx always has a target to swap into
    - In later labs, htmx will swap this entire element using hx-swap="outerHTML"

    Model:
    - string? message - The message to display, or null for empty state
*@

<div id="messages">
    @if (!string.IsNullOrWhiteSpace(Model))
    {
        <div class="alert alert-success" role="alert">
            @Model
        </div>
    }
</div>
````

**Critical design point**: The outer `<div id="messages">` must **always** render, even when there is no message. This ensures htmx always has a valid target element to replace.

### 5.3 Create the Task Form Partial

**File: `Pages/Tasks/Partials/_TaskForm.cshtml`**
````cshtml
@model RazorHtmxWorkshop.Pages.Tasks.IndexModel

@*
    Task Form Fragment
    ==================

    Purpose:
    - Renders the "Create Task" form
    - Handles validation error display
    - Wraps content in a stable #task-form container

    Design notes:
    - We pass the entire IndexModel as the model because we need:
      - Input.Title for the field value
      - ModelState for validation messages
    - The form uses asp-page-handler="Create" which generates ?handler=Create
    - We are NOT adding htmx attributes yet—that comes in Lab 2

    Fragment contract:
    - Root element: <div id="task-form">
    - This ID is the "contract" for htmx targeting
*@

<div id="task-form">
    <form method="post" asp-page-handler="Create">
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
````

**Why pass the full IndexModel:**

The form partial needs access to:
- `Model.Input.Title` - to populate the input field (especially after validation errors)
- `ModelState` - which is accessed via the `asp-validation-for` tag helper

Passing the full model keeps the partial self-contained.

### 5.4 Create the Task List Partial

**File: `Pages/Tasks/Partials/_TaskList.cshtml`**
````cshtml
@using RazorHtmxWorkshop.Models
@model IReadOnlyList<TaskItem>

@*
    Task List Fragment
    ==================

    Purpose:
    - Displays all tasks in a list format
    - Shows empty state when no tasks exist
    - Wraps content in a stable #task-list container

    Design notes:
    - Model is just the list of tasks, not the full PageModel
    - This keeps the partial focused on display logic only
    - The list-group classes are Bootstrap styling

    Fragment contract:
    - Root element: <div id="task-list">
    - This ID is the "contract" for htmx targeting
*@

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
                    <span>@task.Title</span>
                    @if (task.IsDone)
                    {
                        <span class="badge bg-success">Done</span>
                    }
                </li>
            }
        </ul>
    }
</div>
````

**Design decision - Model type:**

This partial receives `IReadOnlyList<TaskItem>` rather than the full `IndexModel` because:
- It only needs the task data to render
- It doesn't need form state or flash messages
- Simpler models are easier to test and reason about

---

## Step 6: Compose the Page with Partials (8–10 minutes)

Now we update the main page to use our new partials instead of inline markup.

### 6.1 Update the Index Page

**File: `Pages/Tasks/Index.cshtml`**
````cshtml
@page
@model RazorHtmxWorkshop.Pages.Tasks.IndexModel
@{
    ViewData["Title"] = "Tasks";
}

@*
    Tasks Page - Fragment Composition
    ==================================

    This page demonstrates the "Fragment First" architecture:
    - Each region is rendered by a dedicated partial
    - Each partial has a stable root element ID
    - The page is a composition of independent fragments

    Fragment inventory:
    - #messages    -> Partials/_Messages (flash message display)
    - #task-form   -> Partials/_TaskForm (create task form)
    - #task-list   -> Partials/_TaskList (task list display)

    In later labs, htmx will target these IDs to swap individual fragments
    without reloading the entire page.
*@

<h1>Tasks</h1>

@*
    Messages Fragment
    -----------------
    Displays success/error messages.
    Model: FlashMessage (string?)
*@
<partial name="Partials/_Messages" model="Model.FlashMessage" />

<div class="row">
    @*
        Form Fragment
        -------------
        Create task form with validation.
        Model: Full IndexModel (needs Input + ModelState)
    *@
    <div class="col-md-5">
        <h2 class="h5">Add a Task</h2>
        <partial name="Partials/_TaskForm" model="Model" />
    </div>

    @*
        List Fragment
        -------------
        Displays all tasks.
        Model: Tasks collection (IReadOnlyList<TaskItem>)
    *@
    <div class="col-md-7">
        <h2 class="h5">Current Tasks</h2>
        <partial name="Partials/_TaskList" model="Model.Tasks" />
    </div>
</div>

@section Scripts {
    <partial name="_ValidationScriptsPartial" />
}
````

### 6.2 Understanding the Partial Tag Helper

The `<partial>` tag helper is the recommended way to render partials in ASP.NET Core:
````cshtml
<partial name="Partials/_Messages" model="Model.FlashMessage" />
````

| Attribute | Purpose |
|-----------|---------|
| `name` | Path to the partial view (relative to current page folder) |
| `model` | Data to pass to the partial's `@model` |

**Path resolution:**
- `"Partials/_Messages"` looks for `Pages/Tasks/Partials/_Messages.cshtml`
- The path is relative to the current page's folder (`Pages/Tasks/`)

### 6.3 Test the Refactored Application

1. **Stop** and **restart** the application
2. **Navigate** to `/Tasks`
3. **Verify** the functionality:
   - Page displays correctly (same as before)
   - Creating tasks works
   - Validation errors display
   - Success messages appear
4. **View the HTML source** to confirm the fragment IDs are present

---

## Step 7: Verification Checklist (2 minutes)

Before moving on to Lab 2, verify your application meets all requirements.

### DOM Verification

Open your browser's Developer Tools (F12) and confirm these elements exist:
````html
<!-- Messages fragment -->
<div id="messages">...</div>

<!-- Form fragment -->
<div id="task-form">...</div>

<!-- List fragment -->
<div id="task-list">...</div>
```

### Functionality Verification

| Test | Expected Result |
|------|-----------------|
| Navigate to `/Tasks` | Page loads with empty task list |
| Submit empty form | Validation error: "Title is required." |
| Enter title, submit | Page reloads, task appears, "Task added." shows |
| Refresh page | Task persists, success message gone (TempData consumed) |

### File Structure Verification

Confirm your project structure matches:
```
RazorHtmxWorkshop/
├── Data/
│   └── InMemoryTaskStore.cs
├── Models/
│   └── TaskItem.cs
├── Pages/
│   ├── Tasks/
│   │   ├── Partials/
│   │   │   ├── _Messages.cshtml
│   │   │   ├── _TaskForm.cshtml
│   │   │   └── _TaskList.cshtml
│   │   ├── Index.cshtml
│   │   └── Index.cshtml.cs
│   └── ... (other pages)
└── ... (other files)
````

---

## Key Takeaways

### The "Fragment First" Mental Model

1. **Partials are not optimization**—they are interfaces for future interactivity
2. **The fragment root element (with `id="..."`) is the contract**—htmx will target these IDs
3. **Each fragment has a single responsibility**—messages, form, or list

### What Comes Next

In **Lab 2**, you will add htmx to this foundation:

- The create form will submit via `hx-post` and receive an updated `_TaskList` fragment
- Only `#task-list` will update—no full page reload
- You will add a "Refresh list" button using `hx-get`
- Validation errors will swap into `#task-form` using response headers

The "Fragment First" architecture you built in this lab makes all of that possible with minimal changes to your existing code.

---

## Troubleshooting

### Common Issues and Solutions

| Problem | Solution |
|---------|----------|
| **Page not found at /Tasks** | Ensure `Index.cshtml` is in `Pages/Tasks/` folder |
| **Partial not found error** | Check the path in `<partial name="...">` matches actual file location |
| **Tasks disappear on restart** | Expected behavior—data is in-memory only |
| **Validation messages don't show** | Ensure `@section Scripts` includes `_ValidationScriptsPartial` |
| **Model binding not working** | Verify `[BindProperty]` attribute on `Input` property |

### Debug Tips

1. **View Page Source**: Check that all fragment IDs render correctly
2. **Network Tab**: Verify form posts to `?handler=Create`
3. **Breakpoints**: Set breakpoints in `OnGet()` and `OnPostCreate()` to trace execution

---

## Summary

You have successfully completed Lab 1! You now have:

- A Razor Pages application with a clean project structure
- A simple domain model with in-memory storage
- Three partial views representing independent UI fragments
- Stable, predictable element IDs for future htmx targeting

This "Fragment First" architecture is your foundation for building highly interactive server-driven UIs. In Lab 2, you will see how htmx leverages these fragments to create a responsive experience without the complexity of a JavaScript framework.

**Proceed to Lab 2: Partial Updates with hx-get + hx-post →**