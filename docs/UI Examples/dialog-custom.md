---
order: 16
icon: device-desktop
---
# Dialogs with Custom htmx

### Implementing Custom Dialogs with htmx and Hyperscript

The Custom Dialog pattern shows how to build fully interactive "Confirm" and "Prompt" modals from scratch using only htmx and Hyperscript. This approach gives you total control over the styling and animation of your dialogs without requiring a heavy CSS framework like Bootstrap or UIKit for the modal logic.

#### 1. Defining the Custom Modal Styles

In `Index.cshtml`, we define basic CSS for our modal overlay and content. We use CSS transitions to handle the fade-in and slide-down animations.

**`Index.cshtml` (Partial)**
```css
#modal-overlay {
    position: fixed;
    top: 0; left: 0; width: 100%; height: 100%;
    background: rgba(0,0,0,0.5);
    display: flex; align-items: center; justify-content: center;
    z-index: 1050; opacity: 0;
    transition: opacity 0.3s ease;
}

#modal-overlay.show { opacity: 1; }

.custom-modal {
    background: white; padding: 2rem; border-radius: 0.5rem;
    max-width: 500px; width: 90%;
    transform: translateY(-20px);
    transition: transform 0.3s ease;
}

.show .custom-modal { transform: translateY(0); }
```

#### 2. Triggering the Modal

The main page contains buttons that fetch the modal HTML from the server and inject it into a container.

**`Index.cshtml`**
```html
<button class="btn btn-danger"
        hx-get="?handler=ConfirmModal"
        hx-target="#modal-container"
        hx-trigger="click"
        hx-swap="innerHTML">
    Custom Delete (Confirm)
</button>

<div id="modal-container"></div>
```

#### 3. Managing the Lifecycle with Hyperscript

The partial view uses [Hyperscript](https://hyperscript.org/) (the `_` attribute) to handle animations and cleanup. This replaces the JavaScript logic typically found in UI frameworks.

**`_ConfirmModal.cshtml`**
```razor
<div id="modal-overlay"
     _="on load add .show to me
        on htmx:beforeOnLoad from body
            if detail.xhr.status >= 200 and detail.xhr.status < 300
                remove .show from me
                wait 300ms
                remove me
            end
        end">
    <div class="custom-modal">
        <h5>Confirm Custom Action</h5>
        <p>Are you sure you want to proceed?</p>
        <button type="button" class="btn btn-secondary"
                _="on click remove .show from #modal-overlay then wait 300ms then remove #modal-overlay">
            Cancel
        </button>
        <button type="button" class="btn btn-danger"
                hx-get="?handler=ConfirmAction"
                hx-target="#confirm-result">
            Yes, Do It
        </button>
    </div>
</div>
```
*   `on load add .show to me`: Triggers the CSS transition for fade-in.
*   `on htmx:beforeOnLoad`: When the user clicks "Yes" and the server responds successfully, we reverse the animation, wait for it to finish (`wait 300ms`), and then `remove me` from the DOM.
*   The **Cancel** button uses Hyperscript to perform the same closing animation and cleanup without making a server request.

#### 4. The Backend: C# PageModel

The `Index.cshtml.cs` handles the requests for the modals and the actions they trigger.

**`Index.cshtml.cs`**
```csharp
public class Index : PageModel
{
    // Returns the HTML for the custom confirmation modal
    public IActionResult OnGetConfirmModal() => Partial("_ConfirmModal");

    // Performs the confirmed action
    public IActionResult OnGetConfirmAction()
    {
        return Content($"<div class='alert alert-success'>Custom Action confirmed!</div>");
    }

    // Processes the prompt input
    public IActionResult OnGetPromptAction(string promptValue)
    {
        return Content($"<div class='alert alert-info'>Custom Prompt result: <strong>{promptValue}</strong></div>");
    }
}
```

#### Why this works well

1.  **Zero Dependencies**: You don't need a large UI library to manage your dialogs. htmx handles the content delivery, and Hyperscript handles the interactivity.
2.  **Total Design Control**: Since the CSS is yours, you can make the modals look and behave exactly how you want.
3.  **Clean DOM**: The `remove me` logic ensures that the modal is completely purged from the document once it's closed, keeping your page lightweight.
4.  **Declarative Logic**: The animation and lifecycle logic are defined directly on the elements they affect, making the code much easier to follow than traditional jQuery or vanilla JS modal implementations.