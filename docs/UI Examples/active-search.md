---
order: 25
icon: search
---
# Active Search

### Implementing Active Search with htmx and ASP.NET Core

The Active Search pattern is one of the most popular use cases for htmx, providing a "search-as-you-type" experience without the complexity of a full-blown JavaScript framework. Here's a look at how it's implemented in our `ActiveSearch` demo.

#### The Frontend: Razor & htmx

In `Index.cshtml`, we define a search input that triggers a request to the server as the user types.

```html
<form>
    @Html.AntiForgeryToken()
    <input class="form-control" type="search"
           name="searchText" placeholder="Begin Typing To Search Users..."
           hx-post="@Url.Page("Index", "Search")"
           hx-trigger="keyup changed delay:500ms, search"
           hx-target="#search-results"
           hx-indicator=".htmx-indicator">
</form>

<table class="table">
    <thead>
        <tr><th>Country</th></tr>
    </thead>
    <tbody id="search-results">
        <!-- Results will be injected here -->
    </tbody>
</table>
```

**Key htmx attributes used:**
*   `hx-post`: Tells htmx to make a POST request to the `Search` handler on the current Page.
*   `hx-trigger`: Specifies when to fire the request. We use `keyup changed delay:500ms` so it only fires 500ms after the user stops typing, avoiding excessive server hits.
*   `hx-target`: Tells htmx where to put the returned HTML (in this case, our table body).
*   `hx-indicator`: Shows a loading spinner while the request is in flight.

#### The Backend: C# PageModel

On the server side in `Index.cshtml.cs`, we handle the search request. The `OnPostSearch` method fetches data from an external API (REST Countries) based on the input and returns a partial view.

```csharp
[BindProperty]
public string SearchText { get; set; }
public List<Country> Countries { get; set; }

public async Task<PartialViewResult> OnPostSearch()
{
    Countries = new();
    try
    {
        var response = await _httpClient.GetAsync($"https://restcountries.com/v3.1/name/{SearchText}");
        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadAsStringAsync();
            var json = JsonArray.Parse(result);
            if (json != null)
            {
                foreach (var country in json.AsArray())
                {
                    var name = country?["name"]?["common"]?.ToString();
                    if (name != null) Countries.Add(new(name));
                }
            }
        }
    }
    catch (Exception) { /* Handle errors */ }

    return Partial("_searchResult", Countries);
}
```

#### The Result: Partial View

Finally, `_searchResult.cshtml` renders just the rows needed for the table:

```razor
@model List<Country>

@foreach (var c in Model)
{
    <tr>
        <td>@c.Name</td>
    </tr>
}
```

This approach keeps the logic simple, the state on the server, and the UI fast and responsive.