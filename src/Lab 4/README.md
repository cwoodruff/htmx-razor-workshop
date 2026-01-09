# htmx ASP.NET Core Razor Pages Workshop - Lab 4

This is the Lab 4 version of the Razor Pages app with core UX patterns implemented: modals, confirm flows, history management, and pagination.

## What's New in Lab 4

- **Details pattern**: Load task details into a side panel via `hx-get`
- **Delete with confirmation**: `hx-confirm` for safe destructive actions
- **Filtering**: Live search with `hx-trigger="keyup changed delay:400ms"`
- **Pagination**: Navigate pages with `hx-push-url` for URL state
- **URL state**: Bookmarkable, shareable URLs for filtered/paginated views
- **Transitions**: CSS transitions for smooth swap animations
- **View model**: `TaskListVm` for filtering and pagination state

## Requirements
- .NET SDK **10.0** (or update the project TargetFramework)

## Run
```bash
dotnet restore
dotnet run
```

Then open the displayed URL.

## Pages
- `/` Home
- `/Labs` Lab outline
- `/Tasks` Demo page with all Lab 4 features:
  - `#messages` - Flash messages (success/error)
  - `#task-form` - Form with real-time validation
  - `#task-list` - Paginated, filterable task list
  - `#task-details` - Details panel
  - `#title-validation` - Field-level validation fragment

## Key Files Changed from Lab 3

### `Models/TaskListVm.cs` (New)
- View model with pagination properties: `Page`, `PageSize`, `Total`, `TotalPages`
- Filter support with `Query` property
- Helper properties: `HasPreviousPage`, `HasNextPage`

### `Data/InMemoryTaskStore.cs`
- Added `Find(int id)` method for details lookup
- Added `Delete(int id)` method for task deletion

### `Pages/Tasks/Index.cshtml.cs`
- Added filter/pagination properties: `Query`, `CurrentPage`, `PageSize`, `TotalTasks`
- Updated `OnGet` to support query parameters
- Added `OnGetList(q, page, pageSize)` with filtering and pagination
- Added `OnGetDetails(int id)` for details panel
- Added `OnPostDelete(int id)` with confirmation support
- Returns `TaskListVm` from list operations

### `Pages/Tasks/Index.cshtml`
- Added filter input with `hx-push-url="true"`
- Added details panel section
- Added loading indicator

### `Pages/Tasks/Partials/_TaskList.cshtml`
- Complete rewrite to use `TaskListVm` model
- Added Details and Delete buttons per row
- Added pagination controls with `hx-push-url`
- Shows "Clear filter" when filtering with no results

### `Pages/Tasks/Partials/_TaskDetails.cshtml` (New)
- Details panel fragment showing task properties

### `wwwroot/css/site.css`
- Added transition styles for `#task-list` and `#task-details`
- Added pagination styling
- Added highlight animation for new items

## Testing the Implementation

### Details Pattern
1. Add some tasks
2. Click "Details" on any task
3. Details panel updates without page reload

### Delete with Confirmation
1. Click "Delete" on a task
2. Browser shows confirmation dialog
3. Confirm to delete, cancel to keep

### Filtering
1. Type in the filter input
2. List updates after 400ms pause
3. URL changes to include `?handler=List&q=...`
4. Use browser back/forward to navigate filter history

### Pagination
1. Add more than 5 tasks
2. Pagination controls appear
3. Click page numbers or Previous/Next
4. URL updates with page parameter
5. Back/forward navigates through history

### URL State
1. Filter and paginate to a specific view
2. Copy the URL
3. Paste in new tab—same view loads
4. Bookmark and return later

## htmx Attributes Used

| Attribute | Purpose |
|-----------|---------|
| `hx-get` | Send GET request |
| `hx-post` | Send POST request |
| `hx-target` | Where to swap response |
| `hx-swap` | How to swap (outerHTML, innerHTML) |
| `hx-vals` | Additional JSON parameters |
| `hx-confirm` | Browser confirmation dialog |
| `hx-push-url` | Update browser URL |
| `hx-trigger` | When to fire request |
| `hx-indicator` | Loading indicator |

## Patterns Summary

| Pattern | Key Attributes | Purpose |
|---------|---------------|---------|
| **Details** | `hx-get`, `hx-target` | Load content into panel |
| **Delete** | `hx-post`, `hx-confirm`, `hx-vals` | Safe destructive actions |
| **Filter** | `hx-trigger`, `hx-push-url` | Live search with URL state |
| **Pagination** | `hx-push-url` on links | Navigable, bookmarkable pages |
| **Transitions** | CSS + htmx classes | Visual polish |

## URL State Philosophy

URLs reflect application state:
- Filter query → `?q=buy`
- Current page → `?page=2`
- Page size → `?pageSize=10`

This enables:
- **Bookmarking**: Users save filtered/paginated views
- **Sharing**: Send a link to a specific view
- **History**: Back/forward buttons work correctly
