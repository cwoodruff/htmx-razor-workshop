---
order: 18
icon: multi-select
---
# Drag & Drop with Sorting

### Implementing Drag-and-Drop Sorting with htmx and ASP.NET Core

The Drag-and-Drop Sortable pattern demonstrates how to integrate a specialized JavaScript library like **Sortable.js** with htmx to create a reorderable list that syncs its state with the server automatically. This provides a highly intuitive UI for managing sequences or priorities.

#### 1. The Frontend: Sortable.js & htmx

In `Index.cshtml`, we wrap our list in a form. We use a small Hyperscript block to initialize the Sortable.js library and bridge its events over to htmx.

**`Index.cshtml`**
```html
<form id="sortable-form"
      hx-post="?handler=Reorder"
      hx-trigger="end"
      hx-target="#sortable-list"
      hx-indicator="#reorder-spinner">

    @Html.AntiForgeryToken()

    <div id="sortable-list" class="list-group"
         _="on load or htmx:afterSwap from #sortable-form
                js(me)
                    if (me._sortable) me._sortable.destroy();
                    me._sortable = Sortable.create(me, {
                        animation: 150,
                        handle: '.handle',
                        onEnd: function() {
                            htmx.trigger('#sortable-form', 'end');
                        }
                    });
                end">
        <partial name="_ItemList" model="Model.Items" />
    </div>
</form>
```

**Key htmx and Hyperscript attributes:**
*   `hx-post="?handler=Reorder"`: Specifies the server endpoint to receive the new order.
*   `hx-trigger="end"`: htmx waits for a custom DOM event named `end` to be fired before sending the request.
*   `on load or htmx:afterSwap`: Hyperscript ensures that Sortable.js is initialized (or re-initialized after a swap) on the list element.
*   `onEnd: function() { htmx.trigger(...) }`: Inside the Sortable.js configuration, we manually trigger the `end` event that htmx is listening for whenever a drag operation finishes.

#### 2. The Item List Partial

The list items contain a hidden input. When Sortable.js reorders the DOM elements, these inputs move with them. When htmx submits the form, it sends these IDs in their new order.

**`_ItemList.cshtml`**
```razor
@model List<Item>

@foreach (var item in Model)
{
    <div class="list-group-item d-flex align-items-center">
        <input type="hidden" name="itemIds" value="@item.Id" />
        <i class="fas fa-grip-vertical handle"></i>
        <span>@item.Name</span>
        <span class="ml-auto">Order: @item.Order</span>
    </div>
}
```

#### 3. The Backend: C# PageModel

The server receives an array of integers representing the IDs in the order they appear in the DOM. It updates the database (or in-memory store) accordingly and returns the updated list fragment.

**`Index.cshtml.cs`**
```csharp
public class Index : PageModel
{
    public List<Item> Items { get; set; } = new();

    // Handler for reordering items
    public IActionResult OnPostReorder(int[] itemIds)
    {
        if (itemIds != null)
        {
            for (int i = 0; i < itemIds.Length; i++)
            {
                var id = itemIds[i];
                var item = _db.Items.FirstOrDefault(x => x.Id == id);
                if (item != null)
                {
                    item.Order = i + 1; // Update sequence based on array index
                }
            }
        }

        Items = _db.Items.OrderBy(i => i.Order).ToList();

        // Return only the partial view to update the UI with new order numbers
        return Partial("_ItemList", Items);
    }
}
```

#### Why this works well

1.  **Best of Both Worlds**: You use a mature, specialized library (Sortable.js) for complex touch/drag interactions, but keep the data synchronization logic in htmx.
2.  **No Manual JSON Mapping**: Because Sortable.js rearranges the actual `<input>` elements in the DOM, htmx's standard form submission naturally sends the correct sequence without any custom data mapping.
3.  **Visual Feedback**: By targeting the list body and returning a partial, you can update "Order" badges or other sequence-dependent UI elements immediately after the drop.
4.  **Resilient Lifecycle**: Using Hyperscript's `htmx:afterSwap` trigger ensures the drag-and-drop functionality continues working even after the list has been updated and replaced by htmx.