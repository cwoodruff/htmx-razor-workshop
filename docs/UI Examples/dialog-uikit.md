---
order: 15
icon: device-desktop
---
# Dialogs with UIKit

### Implementing UIKit Dialogs with htmx and ASP.NET Core

The Dialog UIKit pattern demonstrates how to integrate htmx with the **UIKit** CSS framework to create interactive, server-driven "Confirm" and "Prompt" dialogs. By fetching modal content on-demand and using Hyperscript to manage the UIKit lifecycle, you can create a seamless and responsive user experience.

#### 1. Triggering the UIKit Modal

In `Index.cshtml`, we use standard htmx attributes to request the modal content and inject it into a dedicated container.

**`Index.cshtml`**
```html
<button class="uk-button uk-button-danger"
        hx-get="?handler=ConfirmModal"
        hx-target="#modal-container"
        hx-trigger="click"
        hx-swap="innerHTML">
    Open UIKit Confirm
</button>

<div id="modal-container"></div>
```
*   `hx-get="?handler=ConfirmModal"`: Fetches the partial view for the modal.
*   `hx-target="#modal-container"`: Targets the container where the modal will be injected.
*   `hx-swap="innerHTML"`: Replaces the contents of the container with the new modal HTML.

#### 2. The Modal Lifecycle with Hyperscript

Because UIKit modals require JavaScript to initialize and display, we use [Hyperscript](https://hyperscript.org/) (the `_` attribute) to handle these events declaratively.

**`_ConfirmModal.cshtml`**
```razor
<div id="confirm-modal" class="uk-flex-top" uk-modal
     _="on load call UIkit.modal(me).show() end
        on htmx:beforeOnLoad from body
            if detail.xhr.status >= 200 and detail.xhr.status < 300
                call UIkit.modal(me).hide()
            end
        end
        on hidden remove me">
    <div class="uk-modal-dialog uk-modal-body uk-margin-auto-vertical">
        <h2 class="uk-modal-title">Confirm Action</h2>
        <p>Are you sure you want to proceed?</p>
        <p class="uk-text-right">
            <button class="uk-button uk-button-default uk-modal-close" type="button">Cancel</button>
            <button class="uk-button uk-button-primary" type="button"
                    hx-get="?handler=ConfirmAction"
                    hx-target="#confirm-result">
                Confirm
            </button>
        </p>
    </div>
</div>
```
*   `on load call UIkit.modal(me).show()`: Shows the modal as soon as it's injected into the DOM.
*   `on htmx:beforeOnLoad`: Hides the modal immediately after the confirmation action is successfully processed.
*   `on hidden remove me`: Automatically removes the modal element from the DOM once it's finished closing, preventing "DOM clutter."

#### 3. Handling Prompts and Input

The Prompt pattern follows the same logic but includes a form to capture user input.

**`_PromptModal.cshtml`**
```razor
<form hx-get="?handler=PromptAction" hx-target="#prompt-result">
    <div class="uk-margin">
        <label class="uk-form-label" for="promptValue">Enter your message:</label>
        <input class="uk-input" id="promptValue" name="promptValue" type="text" required autofocus>
    </div>
    <p class="uk-text-right">
        <button class="uk-button uk-button-default uk-modal-close" type="button">Cancel</button>
        <button class="uk-button uk-button-primary" type="submit">Submit</button>
    </p>
</form>
```
*   When the form is submitted, htmx sends the `promptValue` to the server, and the Hyperscript on the parent `div` handles closing the dialog.

#### 4. The Backend: C# PageModel

The `Index.cshtml.cs` file contains the handlers for returning the modals and processing the actions.

**`Index.cshtml.cs`**
```csharp
public class Index : PageModel
{
    // Returns the Modal partials
    public IActionResult OnGetConfirmModal() => Partial("_ConfirmModal");
    public IActionResult OnGetPromptModal() => Partial("_PromptModal");

    // Processes the confirmation
    public IActionResult OnGetConfirmAction()
    {
        return Content("<div class='uk-alert-success' uk-alert><p>Action confirmed!</p></div>");
    }

    // Processes the prompt input
    public IActionResult OnGetPromptAction(string promptValue)
    {
        return Content($"<div class='uk-alert-primary' uk-alert><p>Received: <strong>{promptValue}</strong></p></div>");
    }
}
```

#### Why this works well

1.  **Framework Flexibility**: This demonstrates that htmx isn't tied to any specific CSS framework; it works just as well with UIKit as it does with Bootstrap.
2.  **Declarative Interactivity**: Using Hyperscript allows you to keep your modal logic right in the HTML, avoiding the need for separate, complex JavaScript files to manage modal states.
3.  **On-Demand Loading**: Modals are only loaded when needed, reducing the initial weight of the page and keeping the DOM lean.
4.  **Clean State Management**: By removing the modal from the DOM on `hidden`, you ensure that every "Open" click starts with a fresh, clean state.
