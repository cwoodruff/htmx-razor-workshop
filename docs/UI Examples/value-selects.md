---
order: 12
icon: shield-check
---
# Value Select

### Implementing Cascading Selects with htmx and ASP.NET Core

The Cascading Selects pattern (also known as dependent selects) is a common UI requirement where the options in one dropdown list depend on the selection made in a previous dropdown. htmx makes this incredibly simple by allowing you to fetch and swap the dependent dropdown's content asynchronously.

#### The Frontend: Razor & htmx

In `Index.cshtml`, we have two select elements. The first select (Make) is configured to trigger an htmx request whenever its value changes.

**`Index.cshtml`**
```html
<div>
    <label class="control-label">Make</label>
    <select name="make"
            hx-get="@Url.Page("Index", "Models")"
            hx-target="#models"
            hx-indicator=".htmx-indicator">
        @foreach (var make in Model.ManufacturerMake)
        {
            <option value="@make">@make</option>
        }
    </select>
</div>

<div>
    <label class="control-label">Model</label>
    <select id="models" name="model">
        <partial name="_modelSelector" model="@Model.ManufacturerModels"/>
    </select>
</div>
```

**Key htmx attributes used:**
*   `hx-get`: Initiates a GET request to the `Models` handler when the user selects a different "Make".
*   `hx-target="#models"`: Specifies that the HTML returned by the server (the new list of options) should be placed inside the element with the ID `models`.
*   `name="make"`: htmx automatically includes the value of the select element in the request as a query parameter named `make`.

#### The Backend: C# PageModel

The `IndexModel` manages the data for the manufacturers and their corresponding models. It uses a `Dictionary` to store the relationships and provides a handler to return the filtered models.

**`Index.cshtml.cs`**
```csharp
public class IndexModel : PageModel
{
    private static readonly Dictionary<string, List<string>> MakeModel = new()
    {
        { "Audi", new() { "A1", "A4", "A6" } },
        { "Toyota", new() { "Landcruiser", "Tacoma", "Yaris" } },
        { "BMW", new() { "325i", "325ix", "X5" } }
    };

    public List<string> ManufacturerMake { get; set; } = new();
    public List<string> ManufacturerModels { get; set; } = new();

    [FromQuery(Name = "make")]
    public string Make { get; set; } = string.Empty;

    public void OnGet()
    {
        ManufacturerMake = MakeModel.Keys.ToList();
        Make = ManufacturerMake.First();
        ManufacturerModels = MakeModel[Make];
    }

    // This handler returns the partial view for the second select
    public PartialViewResult OnGetModels()
    {
        ManufacturerModels = MakeModel.ContainsKey(Make)
            ? MakeModel[Make]
            : new List<string>();

        return Partial("_modelSelector", ManufacturerModels);
    }
}
```

#### The Result: Partial View

The `_modelSelector.cshtml` partial view simply renders the `<option>` elements for the model dropdown.

**`_modelSelector.cshtml`**
```razor
@model List<string>

<option value="">Select a model</option>
@foreach (var s in Model)
{
    <option value="@s">@s</option>
}
```

#### Why this works well

1.  **Cleaner Logic**: You don't need to ship a massive JSON object containing every possible combination of data to the client.
2.  **Reduced Complexity**: There is no need for custom JavaScript to clear, filter, or rebuild the second dropdown.
3.  **Server-Side Control**: The server remains the source of truth for the available options, making it easy to integrate with a database or external API.
4.  **Instant Feedback**: The UI updates immediately upon selection, providing a smooth and responsive experience for the user.