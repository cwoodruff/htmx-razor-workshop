---
order: 20
icon: shield-check
---
# Delete Row

### Implementing Row Deletion with htmx and ASP.NET Core

The Delete Row pattern is a common requirement in data-driven applications. Using htmx, you can remove a record from the database and concurrently update the UI by removing the corresponding row from a table—all without a full page reload.

#### The Frontend: Razor & htmx

In `Index.cshtml`, we set up a table where the `<tbody>` is configured to handle the deletion logic for all its child rows.

**`Index.cshtml`**
```html
<tbody hx-confirm="Are you sure?" hx-target="closest tr" hx-swap="outerHTML swap:1s">
    @foreach (var contact in Model.Contacts)
    {
        <partial name="_TableRow" model="@contact"/>
    }
</tbody>
```

The individual rows are rendered via a partial view. Each row contains a "Delete" button that triggers the htmx request.

**`_TableRow.cshtml`**
```razor
@model Contact

<tr>
    <td>@Model.Name</td>
    <td>@Model.Email</td>
    <td>@Model.Status</td>
    <td>
        <button class="btn btn-danger"
                hx-post="@Url.Page("Index", "Contact", new { Id = Model.Id })"
                hx-include="closest form">
            Delete
        </button>
    </td>
</tr>
```

**Key htmx attributes used:**
*   `hx-confirm`: Automatically triggers a browser confirmation dialog before the request is sent.
*   `hx-post`: Sends a POST request to the `Contact` handler with the specific contact's ID.
*   `hx-target="closest tr"`: (Defined on the parent `tbody`) Tells htmx to target the table row containing the clicked button.
*   `hx-swap="outerHTML swap:1s"`: Replaces the entire row with the server response (which is empty in this case). The `swap:1s` allows for a CSS transition effect (like a fade-out) before the element is finally removed from the DOM.
*   `hx-include="closest form"`: Ensures the Anti-Forgery Token from the form is included in the POST request.

#### The Backend: C# PageModel

The `IndexModel` handles the deletion request. When the record is successfully deleted from the service, it returns an `OkResult`. Since the response body is empty, htmx simply removes the target element (the row) from the page.

**`Index.cshtml.cs`**
```csharp
[ValidateAntiForgeryToken]
public class IndexModel(IContactService contactService) : PageModel
{
    public IList<Contact>? Contacts { get; set; }
    [FromQuery(Name = "Id")] public int Id { get; set; }

    public void OnGet()
    {
        this.Contacts = contactService.Get().ToArray();
    }

    public IActionResult OnPostContact()
    {
        // Delete the contact from the data store
        contactService.Delete(this.Id);

        // Returning an empty OkResult tells htmx to
        // remove the target element from the DOM.
        return new OkResult();
    }
}
```

#### Why this works well

1.  **Declarative UI**: You define the confirmation and targeting logic once on the container (`<tbody>`), making the individual row templates cleaner.
2.  **Visual Feedback**: By using `swap:1s`, you can use CSS transitions to provide a smooth removal animation, making the UI feel more polished.
3.  **Low Overhead**: The server only needs to return a simple `200 OK` status, minimizing bandwidth and processing time.
4.  **Security**: The use of `[ValidateAntiForgeryToken]` and `hx-include="closest form"` ensures that the deletion requests are protected against CSRF attacks.