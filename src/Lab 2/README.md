# htmx ASP.NET Core Razor Pages Workshop - Lab 2

This is the Lab 2 version of the Razor Pages app with htmx partial updates implemented.

## What's New in Lab 2

- **hx-post on forms**: Form submission via AJAX without page reload
- **hx-get for refresh**: Fetch fresh data with buttons
- **hx-vals**: Pass parameters without forms
- **Loading indicators**: Visual feedback during requests
- **Error handling**: Retarget responses for validation and server errors
- **Form clearing**: Use HX-Trigger to clear form after success

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
- `/Tasks` Demo page with htmx enhancements:
  - `#messages` - Flash messages and error display
  - `#task-form` - Form with hx-post (retargeted on validation error)
  - `#task-list` - List with refresh buttons

## Key Files Changed from Lab 1

### `Pages/Tasks/Index.cshtml.cs`
- Added `IsHtmx()` helper method
- Added `Fragment()` helper method
- `OnPostCreate` returns fragments for htmx requests
- Added `OnGetList(int? take)` handler
- Added `OnGetEmptyForm()` handler

### `Pages/Tasks/Index.cshtml`
- Added refresh buttons with hx-get
- Added loading indicator
- Added invisible listener for form clearing

### `Pages/Tasks/Partials/_TaskForm.cshtml`
- Added hx-post, hx-target, hx-swap, hx-indicator attributes

### `Pages/Tasks/Partials/_Error.cshtml` (New)
- Error display fragment

### `wwwroot/css/site.css`
- Added htmx indicator styles

## Testing the Implementation

1. **Submit a task**: No page reload, only list updates
2. **Submit empty form**: Validation error appears in form
3. **Type "boom"**: Triggers server error in messages area
4. **Click "Refresh All"**: Fetches all tasks
5. **Click "Top 5"**: Fetches only 5 tasks (uses hx-vals)

## htmx Attributes Used

| Attribute | Purpose |
|-----------|---------|
| `hx-post` | Send POST request |
| `hx-get` | Send GET request |
| `hx-target` | Where to swap response |
| `hx-swap` | How to swap (outerHTML) |
| `hx-vals` | Additional parameters |
| `hx-indicator` | Loading indicator |
| `hx-trigger` | Event listener |

## Response Headers Used

| Header | Purpose |
|--------|---------|
| `HX-Retarget` | Override target for this response |
| `HX-Reswap` | Override swap strategy |
| `HX-Trigger` | Fire custom event |
