# htmx ASP.NET Core Razor Pages Workshop - Lab 5

This is the Lab 5 version of the Razor Pages app with dynamic forms, polling, and out-of-band swaps.

## What's New in Lab 5

- **Dynamic tag rows**: Add/remove form inputs dynamically via `hx-swap="beforeend"` and client-side `remove()`
- **Dependent dropdowns**: Category → Subcategory cascading selection
- **Long-running operations**: Polling with `hx-trigger="every 1s"` for progress updates
- **Out-of-band swaps**: Update multiple page regions from a single response via `hx-swap-oob="true"`
- **Conditional polling**: Server controls whether polling continues by what it renders

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
- `/Tasks` Demo page with all Lab 5 features:
  - `#messages` - Flash messages and OOB swap target
  - `#task-form` - Form with tags and category dropdowns
  - `#task-list` - Paginated, filterable task list
  - `#task-details` - Details panel
  - `#job-status` - Long-running job with polling

## Key Files for Lab 5

### New Data Classes

| File | Purpose |
|------|---------|
| `Data/CategoryData.cs` | Sample data for dependent dropdowns |
| `Data/JobSimulator.cs` | Simulates background job with progress |

### New Partials

| File | Purpose |
|------|---------|
| `Partials/_TagRow.cshtml` | Single tag input with remove button |
| `Partials/_TagsContainer.cshtml` | Container for dynamic tag rows |
| `Partials/_SubcategorySelect.cshtml` | Subcategory dropdown fragment |
| `Partials/_JobStatus.cshtml` | Job status with conditional polling |
| `Partials/_JobStatusWithOob.cshtml` | Job status + OOB message fragment |

### Updated Files

| File | Changes |
|------|---------|
| `Index.cshtml.cs` | Added `Tags`, `Category`, `Subcategory` to input model; added handlers for tags, subcategories, and jobs |
| `Index.cshtml` | Added job demo section |
| `_TaskForm.cshtml` | Added category dropdowns and tags section |

## Handler Inventory (Lab 5 Additions)

| Handler | Verb | Returns | Purpose |
|---------|------|---------|---------|
| `OnGetAddTag` | GET | `_TagRow` | Add new tag input |
| `OnGetRemoveTag` | GET | Empty | Unused (removal is client-side) |
| `OnGetSubcategories` | GET | `_SubcategorySelect` | Update subcategory dropdown |
| `OnPostStartJob` | POST | `_JobStatus` | Start background job |
| `OnGetJobStatus` | GET | `_JobStatus` or `_JobStatusWithOob` | Poll job progress |
| `OnGetResetJob` | GET | `_JobStatus` (null) | Reset job UI |

## Testing the Implementation

### Dynamic Tags
1. Click "Add Tag" → New input appears
2. Add multiple tags
3. Click × to remove a tag
4. Submit form → Tags are logged to console

### Dependent Dropdowns
1. Select a Category (e.g., "Work")
2. Subcategory updates with relevant options
3. Change Category → Subcategory updates
4. Clear Category → Subcategory becomes disabled

### Polling
1. Click "Start Report Generation"
2. Watch progress bar update every second
3. Check Network tab → requests fire every ~1s
4. Job completes → polling stops automatically
5. Success message appears via OOB swap

### OOB Swaps
1. Start a job and wait for completion
2. Observe: Job card AND messages area update together
3. Check Network response → contains both fragments

## Pattern Summary

| Pattern | Key Technique | When to Use |
|---------|---------------|-------------|
| **Add/Remove Rows** | `hx-swap="beforeend"` + `hx-on:click` | Dynamic sub-collections |
| **Dependent Dropdowns** | `hx-get` on change | Cascading selections |
| **Polling** | `hx-trigger="every Xs"` in fragment | Long-running operations |
| **OOB Swaps** | `hx-swap-oob="true"` on additional fragments | Multi-region updates |

## The Server Controls Everything

In all four patterns, the server decides:
- **What HTML to render** (the fragments)
- **Whether to continue polling** (by including/excluding trigger)
- **What else to update** (via OOB fragments)
- **What name attributes to use** (for model binding)

This is the power of hypermedia: the server remains in control of application state.

## Verification Checklist

### Dynamic Tags
- [x] "Add Tag" button appends new tag input
- [x] Each tag has a working remove button (client-side)
- [x] Multiple tags can be added
- [x] Tags are included when form submits
- [x] Form reset clears all tags

### Dependent Dropdowns
- [x] Selecting a category updates subcategory options
- [x] Changing category updates subcategory again
- [x] Clearing category disables subcategory
- [x] Selected values persist on validation failure

### Polling
- [x] Starting job shows progress card
- [x] Progress updates every second
- [x] Network tab shows polling requests
- [x] Polling stops when job completes
- [x] Success/failure state displays correctly

### OOB Swaps
- [x] Job completion updates both job card and messages
- [x] Single network response contains both fragments
- [x] Messages area shows appropriate alert

## Troubleshooting

| Problem | Likely Cause | Solution |
|---------|--------------|----------|
| Tags not binding | Name mismatch | Ensure all inputs use `name="Input.Tags"` |
| Remove button doesn't work | Script error | Check `hx-on:click` syntax |
| Dropdown doesn't update | Handler parameter mismatch | Use `[FromQuery(Name = "...")]` if names differ |
| Polling doesn't stop | Trigger in completed state | Remove trigger when done |
| OOB swap fails | Target ID doesn't exist | Ensure target element exists |
