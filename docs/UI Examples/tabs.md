---
order: 13
icon: tab
---
# Tabs

### Implementing Tabs with htmx and ASP.NET Core

The Tabs pattern is a classic UI element for organizing content. With htmx, you can create a dynamic tabbing system that loads content asynchronously, reducing the initial page weight and making the interface feel snappy and modern.

#### 1. The Container and Initial Load

In `Index.cshtml`, we define a container for our tabs. We use htmx to automatically load the first tab when the page is ready.

**`Index.cshtml`**
```html
<div class="card-body">
    <div id="tabs"
         hx-get="/Tabs/tab1"
         hx-trigger="load delay:100ms"
         hx-target="#tabs">
    </div>
</div>
```
*   `hx-get="/Tabs/tab1"`: Requests the content for the first tab from the server.
*   `hx-trigger="load delay:100ms"`: Automatically fires the request 100ms after the page loads.
*   `hx-target="#tabs"`: Replaces the content of the `div` with the returned HTML.

#### 2. The Tab Partial View

Each tab is represented by a partial view that includes the tab navigation and the specific content for that tab. When a user clicks a different tab, htmx fetches the corresponding partial view and replaces the entire tab structure.

**`_tab1.cshtml`**
```razor
<div class="tab-list" role="tablist">
    <button hx-get="/Tabs/tab1" class="selected" role="tab">Tab 1</button>
    <button hx-get="/Tabs/tab2" role="tab">Tab 2</button>
    <button hx-get="/Tabs/tab3" role="tab">Tab 3</button>
</div>

<div id="tab-content" role="tabpanel" class="tab-content">
    <p>Content for Tab 1...</p>
</div>
```
*   `hx-get`: Each button is wired to fetch its respective tab content.
*   `class="selected"`: The active tab is styled differently to provide visual feedback.
*   Because the `hx-target="#tabs"` was defined on the parent container in `Index.cshtml`, and htmx attributes are inherited, clicking any button in the partial will replace the content of the `#tabs` div.

#### 3. The Backend: C# PageModel

The server-side logic is straightforward. The `IndexModel` provides handlers for each tab, returning the appropriate partial view fragment.

**`Index.cshtml.cs`**
```csharp
public class IndexModel : PageModel
{
    public void OnGet() { }

    // Returns the fragment for Tab 1
    public PartialViewResult OnGetTab1()
    {
        return Partial("_tab1");
    }

    // Returns the fragment for Tab 2
    public PartialViewResult OnGetTab2()
    {
        return Partial("_tab2");
    }

    // Returns the fragment for Tab 3
    public PartialViewResult OnGetTab3()
    {
        return Partial("_tab3");
    }
}
```

#### Why this works well

1.  **Lazy Loading**: Content for secondary tabs is only fetched when requested, speeding up the initial page load for heavy content.
2.  **Simplified State**: You don't need to manage "active" classes or "hidden" attributes in JavaScript. The server simply returns the HTML for the tab in its "active" state.
3.  **Encapsulation**: Each tab's logic and view are contained within its own partial view, making the code easier to maintain and extend.
4.  **No Client-Side Routing**: You get the benefits of dynamic content switching without the complexity of a client-side router or a heavy SPA framework.