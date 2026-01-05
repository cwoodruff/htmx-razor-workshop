---
order: 14
icon: device-desktop
---
# Dialogs with Bootstrap

### Implementing Browser-Style Dialogs with htmx and Bootstrap

The Dialog Browser pattern demonstrates how to replace native browser dialogs like `confirm()` and `prompt()` with rich, custom-styled Bootstrap modals using htmx. This allows you to maintain a consistent UI/UX while still leveraging server-side logic for confirmation and input processing.

#### 1. Triggering the Modal

In `Index.cshtml`, we have buttons that request the modal content from the server. Instead of the modal being hidden in the DOM, it is fetched on-demand.

**`Index.cshtml`**
```html
<!-- Trigger for Confirm Dialog -->
<button class="btn btn-danger"
        hx-get="?handler=ConfirmModal"
        hx-target="#modal-container"
        hx-trigger="click"
        hx-swap="innerHTML">
    Delete Something (Confirm)
</button>

<!-- Container where modals are injected -->
<div id="modal-container"></div>
```
*   `hx-get="?handler=ConfirmModal"`: Calls the server to get the modal HTML fragment.
*   `hx-target="#modal-container"`: Injects the modal into a dedicated container at the bottom of the page.

#### 2. The Modal Fragment (with Hyperscript)

The partial view contains the Bootstrap modal structure. We use a small amount of [Hyperscript](https://hyperscript.org/) (the `_` attribute) to handle the Bootstrap lifecycle (showing the modal and removing it from the DOM after it's hidden).

**`_ConfirmModal.cshtml`**
```razor
<div class="modal fade" id="confirmModal" tabindex="-1" role="dialog"
     _="on load js(me) new bootstrap.Modal(me).show() end
        on htmx:beforeOnLoad from body
            if detail.xhr.status >= 200 and detail.xhr.status < 300
                js(me) bootstrap.Modal.getInstance(me).hide() end
            end
        end
        on hidden.bs.modal remove me">
    <div class="modal-dialog">
        <div class="modal-content">
            <div class="modal-header">
                <h5 class="modal-title">Confirm Deletion</h5>
                <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
            </div>
            <div class="modal-body">
                Are you sure you want to delete this item?
            </div>
            <div class="modal-footer">
                <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Cancel</button>
                <button type="button" class="btn btn-danger"
                        hx-get="?handler=ConfirmAction"
                        hx-target="#confirm-result">
                    Yes, Delete It
                </button>
            </div>
        </div>
    </div>
</div>
```
*   **Hyperscript logic**: On `load`, it initializes and shows the Bootstrap modal. On `htmx:beforeOnLoad` (when the "Yes" action finishes successfully), it hides the modal. On `hidden.bs.modal`, it cleans up by removing itself from the DOM.
*   `hx-get="?handler=ConfirmAction"`: The actual action performed once the user confirms.

#### 3. Handling Prompts

The Prompt pattern is similar but uses a form inside the modal to capture user input.

**`_PromptModal.cshtml`**
```razor
<form hx-get="?handler=PromptAction" hx-target="#prompt-result">
    <div class="modal-body">
        <label for="promptValue" class="form-label">Name:</label>
        <input type="text" class="form-control" id="promptValue" name="promptValue" required autofocus>
    </div>
    <div class="modal-footer">
        <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Cancel</button>
        <button type="submit" class="btn btn-primary">Submit</button>
    </div>
</form>
```
*   When the form is submitted, htmx sends the `promptValue` to the server, updates the target on the main page, and the Hyperscript on the wrapping `div` handles closing the modal.

#### 4. The Backend: C# PageModel

The `Index.cshtml.cs` handles returning the partial views for the modals and the final actions triggered from within them.

**`Index.cshtml.cs`**
```csharp
public class Index : PageModel
{
    // Returns the Modal HTML
    public IActionResult OnGetConfirmModal() => Partial("_ConfirmModal");

    // Performs the confirmed action
    public IActionResult OnGetConfirmAction()
    {
        return Content("<div class='alert alert-success'>Successfully confirmed!</div>");
    }

    // Processes the prompt input
    public IActionResult OnGetPromptAction(string promptValue)
    {
        return Content($"<div class='alert alert-info'>You entered: <strong>{promptValue}</strong></div>");
    }
}
```

#### Why this works well

1.  **UX Consistency**: You can style these dialogs to perfectly match your application's theme, unlike native browser dialogs.
2.  **Server-Side Templates**: You don't need to write complex JavaScript to build modals or manage their state; the server provides the HTML.
3.  **Automatic Cleanup**: Using Hyperscript to `remove me` on hidden ensures that your DOM doesn't get cluttered with orphaned modal elements.
4.  **Native Fallback**: For simple cases, you can still use the built-in `hx-confirm="Message"` attribute for a quick native browser confirmation without any extra templates.