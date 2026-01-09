# htmx ASP.NET Core Razor Pages Workshop - Lab 3

This is the Lab 3 version of the Razor Pages app with real-time validation implemented.

## What's New in Lab 3

- **Data annotations**: `[Required]`, `[StringLength]` on input models
- **Real-time validation**: `hx-trigger="keyup changed delay:500ms"` for debounced validation
- **Field-level fragments**: Tiny `_TitleValidation` partial for single-field errors
- **Full form validation**: `TryValidateModel` with complete form retargeting
- **Antiforgery handling**: `hx-include="closest form"` ensures token is sent
- **Success messaging**: `HX-Trigger: showMessage,clearForm` for event-driven updates
- **Form reset**: Listener elements respond to server-triggered events

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
- `/Tasks` Demo page with real-time validation:
  - `#messages` - Flash messages (success/error)
  - `#task-form` - Form with real-time validation
  - `#task-list` - Task list with refresh buttons
  - `#title-validation` - Field-level validation fragment

## Key Files Changed from Lab 2

### `Pages/Tasks/Index.cshtml.cs`
- Added `System.ComponentModel.DataAnnotations` import
- Added `[Required]` and `[StringLength]` to `NewTaskInput`
- Added `OnPostValidateTitle()` handler for keystroke validation
- Added `OnGetMessages()` handler for success messages
- Updated `OnPostCreate` to use `TryValidateModel` and trigger events

### `Pages/Tasks/Index.cshtml`
- Added event listener divs for `showMessage` and `clearForm`

### `Pages/Tasks/Partials/_TaskForm.cshtml`
- Added `@Html.AntiForgeryToken()`
- Added `asp-validation-summary="ModelOnly"`
- Added htmx attributes on input: `hx-post`, `hx-trigger`, `hx-target`, `hx-include`
- Added `_TitleValidation` partial inclusion

### `Pages/Tasks/Partials/_TitleValidation.cshtml` (New)
- Field-level validation fragment

### `Pages/Tasks/Partials/_Messages.cshtml`
- Updated to use dismissible success alerts

### `wwwroot/css/site.css`
- Added real-time validation visual feedback styles

## Testing the Implementation

### Real-Time Validation
1. Start typing in the Title field
2. Wait 500ms after typing stops
3. Validation message appears below input
4. Error clears when input becomes valid

### Full-Form Validation
1. Submit with invalid input
2. Form replaces with error messages
3. Both `asp-validation-for` and htmx validation show

### Success Flow
1. Submit with valid title (3+ characters)
2. Task appears in list
3. Success message shows
4. Form clears automatically

### Network Tab Verification
- Keystroke: POST to `?handler=ValidateTitle` (small response)
- Submit: POST to `?handler=Create` (list response)
- Success triggers: GET to `?handler=Messages` and `?handler=EmptyForm`

## htmx Attributes Used

| Attribute | Purpose |
|-----------|---------|
| `hx-post` | Send POST request |
| `hx-trigger` | When to fire (`keyup changed delay:500ms`) |
| `hx-target` | Where to swap response |
| `hx-swap` | How to swap (outerHTML) |
| `hx-include` | Include form fields (antiforgery token) |
| `hx-indicator` | Loading indicator |

## Two Validation Granularities

| Type | Trigger | Target | Fragment | Use Case |
|------|---------|--------|----------|----------|
| Micro | Keystroke (debounced) | `#title-validation` | `_TitleValidation` | Instant feedback |
| Full | Form submit | `#task-form` | `_TaskForm` | Complete validation |

## Event-Driven Updates

| Event | Listener Action | Result |
|-------|-----------------|--------|
| `showMessage` | GET `?handler=Messages` | Success message appears |
| `clearForm` | GET `?handler=EmptyForm` | Form resets for next entry |
