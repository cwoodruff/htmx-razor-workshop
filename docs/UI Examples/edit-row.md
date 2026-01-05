---
order: 19
icon: shield-check
---
# Edit Row

### Implementing Row Editing with htmx and ASP.NET Core

The Edit Row pattern allows users to edit individual rows in a table without leaving the page or opening a modal. By leveraging htmx, we can swap a static table row for an editable version and then swap it back once the update is complete.

#### 1. The Table Structure

In `Index.cshtml`, we define a table where the `<tbody>` is configured to handle the swap target for any htmx request originating from its children.

**`Index.cshtml`**
```html
<tbody hx-target="closest tr" hx-swap="outerHTML">
    @foreach (var contact in Model.Contacts)
    {
        <partial name="_TableRow" model="@contact"/>
    }
</tbody>
```
*   `hx-target="closest tr"`: This tells htmx that the result of any request within this body should target the nearest table row.
*   `hx-swap="outerHTML"`: Replaces the entire `<tr>` element with the response from the server.

#### 2. The Read-Only Row

Initially, each contact is rendered using a read-only partial view. The "Edit" button triggers a GET request to fetch the editable version of that specific row.

**`_TableRow.cshtml`**
```razor
@model Contact

<tr>
    <td>@Model.Name</td>
    <td>@Model.Email</td>
    <td>
        <button class="btn btn-primary"
                hx-get="@Url.Page("Index", "Edit", new { Id = Model.Id })">
            Edit
        </button>
    </td>
</tr>
```

#### 3. The Editable Row

When the "Edit" button is clicked, the server returns the `_EditRow.cshtml` partial. This row contains input fields and action buttons.

**`_EditRow.cshtml`**
```razor
@model Contact

<tr>
    <td>
        <input type="hidden" asp-for="@Model.Id"/>
        <input asp-for="@Model.Name" class="form-control">
        @Html.AntiForgeryToken()
    </td>
    <td>
        <input asp-for="@Model.Email" class="form-control">
    </td>
    <td>
        <button class="btn btn-secondary" hx-get="@Url.Page("Index", "View", new { Id = Model.Id })">
            Cancel
        </button>
        <button class="btn btn-success"
                hx-put="@Url.Page("Index", "Update", new { Id = Model.Id })"
                hx-include="closest tr">
            Save
        </button>
    </td>
</tr>
```
*   `hx-include="closest tr"`: Ensures that the values of the input fields in the current row are included in the PUT request.
*   The "Cancel" button simply fetches the read-only row again, discarding any unsaved changes.

#### 4. The Backend: C# PageModel

The `IndexModel` handles the transitions between states. It provides handlers for entering edit mode, canceling/viewing, and performing the update.

**`Index.cshtml.cs`**
```csharp
public class IndexModel(IContactService contactService) : PageModel
{
    public IList<Contact>? Contacts { get; set; }

    public void OnGet()
    {
        this.Contacts = contactService.Get().ToArray();
    }

    // Returns the editable row fragment
    public PartialViewResult OnGetEdit(int Id)
    {
        var contact = contactService.Get(Id);
        return Partial("_EditRow", contact);
    }

    // Returns the read-only row fragment (used for Cancel)
    public PartialViewResult OnGetView(int Id)
    {
        var contact = contactService.Get(Id);
        return Partial("_TableRow", contact);
    }

    // Handles the update and returns the updated read-only row
    public PartialViewResult OnPutUpdate([FromForm] Contact contact)
    {
        contactService.Update(contact);
        return Partial("_TableRow", contact);
    }
}
```

#### Summary

This pattern keeps the user in the flow of their data. By targeting only the "closest tr", htmx makes it easy to manage row-level state without complex JavaScript selectors or managing a global state object. The transition between "view" and "edit" modes is seamless and extremely responsive.