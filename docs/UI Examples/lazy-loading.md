---
order: 15
icon: shield-check
---
# Lazy Loading

# Boosting Page Performance: Lazy Loading with htmx and ASP.NET Core Razor Pages

In modern web development, speed is everything. A slow-loading dashboard can frustrate users and lead to higher bounce rates. One of the most effective techniques to improve perceived performance is **Lazy Loading**—deferring the loading of non-critical, heavy components until after the main page is ready.

In this post, we’ll explore how to implement lazy loading in an ASP.NET Core Razor Pages project using [htmx](https://htmx.org/).

## The Scenario
Imagine a "Sales Performance Dashboard" with two heavy components:
1.  **Sales by Region Chart**: Requires complex data aggregation.
2.  **Recent Activity Feed**: Fetches data from a slow external API.

Instead of making the user wait for these components to load before seeing the page, we’ll load the shell immediately and pull the data in asynchronously.

## 1. The Backend: Named Page Handlers
In Razor Pages, we can use **Named Handlers** to return partial views independently. In our `Index.cshtml.cs`, we define two handlers: `OnGetSalesChart` and `OnGetRecentActivity`.

```csharp
public class IndexModel : PageModel
{
    public void OnGet() { /* Loads initial page shell */ }

    public async Task<IActionResult> OnGetSalesChart()
    {
        // Simulate a slow database or API call
        await Task.Delay(2000);
        return Partial("_SalesChart");
    }

    public async Task<IActionResult> OnGetRecentActivity()
    {
        // Simulate another slow call
        await Task.Delay(1000);
        return Partial("_RecentActivity");
    }
}
```

## 2. The Frontend: htmx Magic
Using htmx, we can trigger these handlers as soon as the page loads using the `hx-trigger="load"` attribute.

In `Index.cshtml`, we set up containers with "skeletons" or loading spinners. htmx will automatically replace these placeholders with the returned HTML once the server responds.

```html
<div class="row">
    <div class="col-md-7">
        <!-- Sales Chart Widget -->
        <div hx-get="/LazyLoading/Index?handler=SalesChart"
             hx-trigger="load">
            <div class="text-center p-5 border rounded bg-light">
                <div class="spinner-border text-primary" role="status"></div>
                <p>Loading Sales Data...</p>
            </div>
        </div>
    </div>
    <div class="col-md-5">
        <!-- Recent Activity Widget -->
        <div hx-get="/LazyLoading/Index?handler=RecentActivity"
             hx-trigger="load">
            <div class="text-center p-5 border rounded bg-light">
                <div class="spinner-border text-secondary" role="status"></div>
                <p>Checking for updates...</p>
            </div>
        </div>
    </div>
</div>
```

## 3. The Partial Views
The partial views (`_SalesChart.cshtml` and `_RecentActivity.cshtml`) contain only the HTML fragment needed for those specific widgets. For example, the `_SalesChart.cshtml` might look like this:

```html
<div class="card mb-3">
    <div class="card-body text-center">
        <h5 class="card-title">Sales by Region</h5>
        <!-- Simulated Chart -->
        <div class="d-flex align-items-end justify-content-around" style="height: 150px;">
            <div class="bg-primary" style="width: 15%; height: 40%;"></div>
            <div class="bg-info" style="width: 15%; height: 75%;"></div>
            <!-- ... more bars ... -->
        </div>
        <p class="mt-3 text-muted small">Data as of @DateTime.Now.ToLongTimeString()</p>
    </div>
</div>
```

## Why This Works
1.  **Instant Feedback**: The user sees the dashboard layout and navigation immediately.
2.  **Parallel Loading**: Both widgets start loading at the same time, without blocking the main thread.
3.  **No Custom JavaScript**: We achieved a complex asynchronous behavior using only standard ASP.NET Core patterns and a few htmx attributes.

## Conclusion
Lazy loading with htmx and Razor Pages is a powerful combination. It allows you to keep your backend logic in C# while providing the snappy, reactive experience users expect from modern web applications.
