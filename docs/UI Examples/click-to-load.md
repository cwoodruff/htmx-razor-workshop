---
order: 21
icon: shield-check
---
# Click to Load

### Implementing Click-to-Load with htmx and ASP.NET Core

The Click-to-Load pattern is a great alternative to traditional pagination or infinite scroll. It allows users to load more data only when they explicitly request it, keeping the initial page load light while providing a smooth way to browse through larger datasets.

#### The Frontend: Razor & htmx

In `Index.cshtml`, we start with an initial set of data and a "Load More" button (contained within a partial view).

**`Index.cshtml`**
```html
<table>
    <thead>
        <tr>
            <th>Name</th><th>Email</th>
        </tr>
    </thead>
    <tbody>
        <partial name="_ClickToLoadButton" model="@Model.Contacts"/>
    </tbody>
</table>
```

The magic happens in the partial view. When the user clicks the button, htmx fetches the next page and replaces the *entire* row containing the button with the new data and a *new* button for the subsequent page.

**`_ClickToLoadButton.cshtml`**
```razor
@model List<Contact>
@{
    int currentPage = (int)ViewData["PageNumber"];
}

@foreach (var c in Model)
{
    <tr>
        <td>@c.Name</td><td>@c.Email</td>
    </tr>
}

<tr id="replaceMe">
    <td colspan="3">
        <button class='btn' hx-get="/Clicktoload/Index/NextPage?page=@(currentPage + 1)"
                hx-target="#replaceMe"
                hx-swap="outerHTML">
            Load More... <img class="htmx-indicator" src="/img/bars.svg">
        </button>
    </td>
</tr>
```

**Key htmx attributes used:**
*   `hx-get`: Requests the next page of results from the server.
*   `hx-target="#replaceMe"`: Targets the specific table row that currently holds the button.
*   `hx-swap="outerHTML"`: Replaces the target row with the new rows and the next "Load More" button.

#### The Backend: C# PageModel

The `IndexModel` handles the logic for fetching paged data. It uses `ViewData` to keep track of the current page number so the partial view knows what page to request next.

**`Index.cshtml.cs`**
```csharp
public class IndexModel : PageModel
{
    [ViewData] public int PageCount { get; set; } = 5;
    [ViewData] public int PageNumber { get; set; } = 0;
    [FromQuery(Name = "page")] public int NextPage { get; set; }
    public List<Contact>? Contacts { get; set; }

    public void OnGet()
    {
        Contacts = GetPagedResults(PageNumber, PageCount).ToList();
    }

    public PartialViewResult OnGetNextPage()
    {
        PageNumber = NextPage;
        var results = GetPagedResults(NextPage, PageCount).ToList();
        return Partial("_ClickToLoadButton", results);
    }

    private IEnumerable<Contact> GetPagedResults(int page, int take)
    {
        // Simple logic to generate dummy data for the demo
        var start = 10 + (page * take);
        for (int i = start; i < start + take; i++)
        {
            yield return new Contact("User Name", $"user{i}@example.com", Guid.NewGuid());
        }
    }
}
```

#### Why this works well

1.  **Reduced Initial Payload**: You only send the first few records to the browser.
2.  **Server-Side State**: The server dictates what the next "page" is by providing the URL in the new "Load More" button.
3.  **Seamless Integration**: New rows are appended to the table naturally, maintaining the user's scroll position while extending the list.
4.  **Simplicity**: No complex JavaScript state management is required to track offsets or append elements to the DOM.