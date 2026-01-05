---
order: 23
icon: pencil
---
# Bulk Update

### Implementing Bulk Updates with htmx and ASP.NET Core

The Bulk Update pattern demonstrates how to handle multiple record updates efficiently using htmx. This approach allows users to select several items from a list and perform an action on all of them at once, with the UI updating dynamically to reflect the changes.

#### The Frontend: Razor & htmx

In `Index.cshtml`, we have a set of buttons and a table wrapped in a form. The buttons use `hx-include` to pull in the selected checkboxes.

```html
<div hx-include="#checked-contacts" hx-target="#tbody">
    <button class="btn btn-primary" hx-put="@Url.Page("Index", "Activate")">Activate</button>
    <button class="btn btn-primary" hx-put="@Url.Page("Index", "Deactivate")">Deactivate</button>
</div>

<form id="checked-contacts">
    @Html.AntiForgeryToken()
    <table>
        <thead>
            <tr>
                <th></th>
                <th>Name</th>
                <th>Email</th>
                <th>Status</th>
            </tr>
        </thead>
        <tbody id="tbody">
            <partial name="./_tbody" model="@Model.ContactTableRows"/>
        </tbody>
    </table>
</form>
```

**Key htmx attributes used:**
*   `hx-put`: Sends a PUT request to the specified page handler (`OnPutActivate` or `OnPutDeactivate`).
*   `hx-include`: Ensures that the values from the `#checked-contacts` form (specifically the checked checkboxes named `ids`) are included in the request, even though the buttons are outside the form.
*   `hx-target`: Specifies that the returned HTML should replace the content of the `<tbody>` with the ID `tbody`.

#### The Backend: C# PageModel

The `Index.cshtml.cs` file handles the bulk update logic. It receives an array of IDs from the checked checkboxes and updates the corresponding contacts via a service.

```csharp
public IActionResult OnPutActivate(int[] ids)
{
    if (ids != null && ids.Length > 0)
    {
        foreach (var id in ids)
        {
            service.Update(id, true); // Set status to Active
        }
    }

    var models = service.Get();
    // Mark updated items so the UI can highlight them
    foreach (var m in models)
    {
        m.Updated = ids != null && ids.Contains(m.Id);
    }

    return Partial("_tbody", models.ToList());
}
```

#### The Result: Partial View

The `_tbody.cshtml` partial view renders the table rows. It also applies CSS classes to rows that were just updated to provide visual feedback.

```razor
@model List<Contact>

@foreach (var c in Model)
{
    string statusValue = c.Status ? "Active" : "Inactive";
    string statusClass = c.Status ? "activate" : "deactivate";

    <tr class="@(c.Updated ? statusClass : "")">
        <td>
            <input id="@c.Id" type='checkbox' name='ids' value='@c.Id'>
        </td>
        <td>@c.Name</td>
        <td>@c.Email</td>
        <td>@statusValue</td>
    </tr>
}
```

By using `hx-include`, we decouple the action buttons from the form structure, and by returning a partial view, we update only the necessary part of the page, making the application feel much more responsive.