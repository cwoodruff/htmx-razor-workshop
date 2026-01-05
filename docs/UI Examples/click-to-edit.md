---
order: 22
icon: shield-check
---
# Click to Edit

### Implementing Click-to-Edit with htmx and ASP.NET Core

The Click-to-Edit pattern is a sleek way to provide inline editing capabilities. Instead of redirecting to a separate edit page, htmx allows you to swap the display view with an edit form in-place, and then swap back once the update is complete.

#### 1. The Display State

Initially, we show the contact information. The container is configured to handle the swap when an inner element triggers an htmx request.

**`_DisplayContactForm.cshtml`**
```razor
@model Contact

<div hx-target="this" hx-swap="outerHTML">
    <div><label asp-for="@Model.FirstName"></label>: @Model.FirstName</div>
    <div><label asp-for="@Model.LastName"></label>: @Model.LastName</div>
    <div><label asp-for="@Model.Email"></label>: @Model.Email</div>
    <button hx-get="@Url.Page("Index", "EditContact")" class="btn btn-primary">
        Click To Edit
    </button>
</div>
```
*   `hx-target="this"`: Tells htmx to replace the current `div` with the response.
*   `hx-swap="outerHTML"`: Ensures the entire `div` is replaced, not just its content.
*   `hx-get`: Requests the edit form from the `EditContact` handler.

#### 2. The Edit State

When the "Click To Edit" button is pressed, the server returns a partial view containing the form.

**`_EditContactForm.cshtml`**
```razor
@model Contact

<form hx-put="@Url.Page("Index", "ReplaceContact")" hx-target="this" hx-swap="outerHTML">
    @Html.AntiForgeryToken()
    <div>
        <label asp-for="@Model.FirstName">First Name</label>
        <input asp-for="@Model.FirstName">
    </div>
    <div>
        <label asp-for="@Model.LastName">Last Name</label>
        <input asp-for="@Model.LastName">
    </div>
    <div>
        <label asp-for="@Model.Email">Email Address</label>
        <input asp-for="@Model.Email">
    </div>
    <button class="btn btn-primary">Submit</button>
    <button class="btn btn-secondary" hx-get="@Url.Page("Index", "DisplayContact")">Cancel</button>
</form>
```
*   `hx-put`: Submits the form data to the `ReplaceContact` handler.
*   The "Cancel" button uses `hx-get` to fetch the display version again, discarding changes.

#### 3. The Backend: C# PageModel

The `Index.cshtml.cs` file manages the transitions between these states by returning the appropriate partial views.

**`Index.cshtml.cs`**
```csharp
public class Index : PageModel
{
    private IContactService contactService;
    // ... constructor ...

    // Fetches the edit form
    public PartialViewResult OnGetEditContact()
    {
        var contact = contactService.Get(1);
        return Partial("_EditContactForm", contact);
    }

    // Handles the update and returns the display view
    public IActionResult OnPutReplaceContact(Contact model)
    {
        contactService.Update(1, model);
        return Partial("_DisplayContactForm", model);
    }

    // Handles cancellation
    public PartialViewResult OnGetDisplayContact()
    {
        var contact = contactService.Get(1);
        return Partial("_DisplayContactForm", contact);
    }
}
```

#### Summary

This pattern provides a very "app-like" feel. By swapping small fragments of HTML, you avoid full page reloads and maintain the user's scroll position and context, all while keeping your logic cleanly separated in Razor Pages and C#.