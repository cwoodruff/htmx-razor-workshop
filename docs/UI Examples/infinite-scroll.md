---
order: 17
icon: infinity
---
# Infinite Scroll

### Implementing Infinite Scroll with htmx and ASP.NET Core

The Infinite Scroll pattern provides a seamless browsing experience where new content is automatically loaded as the user reaches the bottom of the list. This is a popular alternative to traditional pagination, often seen in social media feeds and product listings.

#### The Frontend: Razor & htmx

In `Index.cshtml`, we start with a standard table. The initial set of rows is rendered via a partial view.

**`Index.cshtml`**
```html
<table class="table table-bordered">
    <thead>
        <tr>
            <th>Name</th><th>Email</th><th>ID</th>
        </tr>
    </thead>
    <tbody>
        <partial name="_PageResult" model="@Model.Contacts"/>
    </tbody>
</table>
```

The key logic resides in the `_PageResult.cshtml` partial. It iterates through the contacts and specifically marks the **last row** of the current set with htmx attributes to trigger the next load.

**`_PageResult.cshtml`**
```razor
@model List<Contact>
@{
    int currentPage = (int)ViewData["PageNumber"];
}

@if (Model.Count > 0)
{
    int totalCount = Model.Count;
    for (int count = 0; count < totalCount; count++)
    {
        if ((count + 1) == totalCount)
        {
            @* The last row triggers the next page load when it is revealed *@
            <tr hx-get="/InfiniteScroll/Index?handler=NextPage&Page=@(currentPage + 1)"
                hx-trigger="revealed"
                hx-swap="afterend">
                <td>@Model[count].Name</td>
                <td>@Model[count].Email</td>
                <td>@Model[count].UniqueIdentifier</td>
            </tr>
        }
        else
        {
            <tr>
                <td>@Model[count].Name</td>
                <td>@Model[count].Email</td>
                <td>@Model[count].UniqueIdentifier</td>
            </tr>
        }
    }
}
```

**Key htmx attributes used:**
*   `hx-get`: Requests the next page of data from the server.
*   `hx-trigger="revealed"`: This is the "magic" attribute. It tells htmx to fire the request as soon as the element becomes visible in the viewport (i.e., when the user scrolls to it).
*   `hx-swap="afterend"`: Instead of replacing the target, this appends the returned HTML *after* the current element, effectively extending the table rows.

#### The Backend: C# PageModel

The server-side code handles the request for the next page. It tracks the current page number and returns a partial view containing the next batch of contacts.

**`Index.cshtml.cs`**
```csharp
public class IndexModel : PageModel
{
    [ViewData] public int PageCount { get; set; } = 25;
    [ViewData] public int PageNumber { get; set; } = 0;
    [FromQuery(Name = "page")] public int NextPage { get; set; }
    public List<Contact>? Contacts { get; set; }

    public void OnGet()
    {
        this.Contacts = GetPagedResults(PageNumber, PageCount).ToList();
    }

    public PartialViewResult OnGetNextPage()
    {
        PageNumber = NextPage;
        var results = GetPagedResults(NextPage, PageCount).ToList();
        return Partial("_PageResult", results);
    }

    private IEnumerable<Contact> GetPagedResults(int page, int take)
    {
        var start = 10 + (page * take);
        for (int i = start; i < start + take; i++)
        {
            yield return new Contact("User Name", $"user{i}@example.com", Guid.NewGuid());
        }
    }
}
```

#### Why this works well

1.  **Superior UX**: Users can continue reading without having to stop and click a "Next" button.
2.  **Efficient Loading**: Data is only fetched when the user actually scrolls to it, saving bandwidth and server resources for users who only view the top of the list.
3.  **Simple Implementation**: Unlike complex JavaScript solutions that require monitoring scroll events and calculating offsets, htmx handles the intersection observation automatically via the `revealed` trigger.
4.  **Natural Extension**: Using `hx-swap="afterend"` on the last row naturally appends the next set of rows to the existing table structure.