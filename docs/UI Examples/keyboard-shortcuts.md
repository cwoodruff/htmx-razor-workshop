---
order: 17
icon: key-asterisk
---
# Keyboard Shortcuts

### Implementing Keyboard Shortcuts with htmx and Hyperscript

The Keyboard Shortcuts pattern demonstrates how to add global hotkeys to your web application using [Hyperscript](https://hyperscript.org/) and htmx. This provides a "desktop-like" feel, allowing power users to navigate and trigger actions quickly without relying solely on the mouse.

#### 1. Defining Global Shortcuts

In `Index.cshtml`, we define the keyboard shortcuts on the main container using Hyperscript's `_` attribute. We listen for `keydown` events from the global `window` object to ensure they are captured regardless of where the focus is.

**`Index.cshtml`**
```html
<div class="container-fluid"
     _="on keydown[(altKey or metaKey) and shiftKey and key == '1'] from window click #btn1
        on keydown[(altKey or metaKey) and shiftKey and key == '2'] from window click #btn2
        on keydown[(altKey or metaKey) and shiftKey and key == '3'] from window click #btn3
        on keydown[altKey and shiftKey and key == 'S'] from window
            if target.tagName is not 'INPUT' and target.tagName is not 'TEXTAREA'
                focus() #search-input
            end">

    <!-- Action Buttons -->
    <button id="btn1" class="btn btn-success"
            hx-post="?handler=Action1"
            hx-target="#result">
        Action 1 (Alt+Shift+1)
    </button>

    <!-- Search Input -->
    <input type="text" id="search-input" name="q"
           hx-post="?handler=Search"
           hx-target="#result"
           hx-trigger="keyup[key=='Enter']">
</div>
```

**Key components of the Hyperscript logic:**
*   `on keydown[...] from window`: Listens for key presses globally.
*   `[(altKey or metaKey) and shiftKey and key == '1']`: Filters the event. We use both `altKey` (Windows) and `metaKey` (Mac Command key) plus `shiftKey` to avoid overriding default browser shortcuts.
*   `click #btn1`: Programmatically clicks the button, which then triggers its `hx-post` request.
*   `focus() #search-input`: For the "S" shortcut, we explicitly move focus to the search field, but only if the user isn't already inside another input.

#### 2. Local Trigger Filters

htmx also allows you to filter triggers directly on elements. In the search input, we only want to submit the search when the user presses the `Enter` key.

```html
<input type="text" name="q"
       hx-post="?handler=Search"
       hx-trigger="keyup[key=='Enter']">
```
*   `hx-trigger="keyup[key=='Enter']"`: This tells htmx to only perform the POST request if the `keyup` event was specifically for the `Enter` key.

#### 3. The Backend: C# PageModel

The server-side handlers are standard Razor Page methods. They process the request and return an HTML fragment (or a simple string) to be swapped into the target element.

**`Index.cshtml.cs`**
```csharp
public class Index : PageModel
{
    public IActionResult OnPostAction1()
    {
        return Content("<div class='alert alert-success'>Action 1 triggered!</div>");
    }

    public IActionResult OnPostAction2()
    {
        return Content("<div class='alert alert-info'>Action 2 triggered!</div>");
    }

    public IActionResult OnPostSearch(string q)
    {
        return Content($"<div class='alert alert-primary'>Search results for: {q}</div>");
    }
}
```

#### Why this works well

1.  **Declarative Shortcuts**: You don't need to write complex JavaScript event listeners or manage `addEventListener`/`removeEventListener` manually.
2.  **Unified Action Logic**: By triggering a `click` on existing buttons, you reuse the same htmx logic for both mouse and keyboard users.
3.  **Cross-Platform Support**: By checking for `altKey or metaKey`, you provide a consistent experience for both Windows and Mac users.
4.  **Improved Accessibility**: Keyboard shortcuts make the application more efficient for power users and provide an alternative navigation method for users with motor impairments.