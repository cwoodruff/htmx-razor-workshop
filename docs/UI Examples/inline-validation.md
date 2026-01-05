---
order: 16
icon: sync
---
# Inline Validation

### Implementing Inline Validation with htmx and ASP.NET Core

The Inline Validation pattern allows you to provide immediate feedback to users as they fill out a form, without waiting for a full form submission. This is particularly useful for tasks like checking if an email address is already in use or validating complex input requirements.

#### The Frontend: Razor & htmx

In the `InlineValidation` demo, the main form includes a partial view for the email field. This field is configured to validate itself independently.

**`Index.cshtml`**
```html
<form method="post">
    <partial name="_EmailField" model="@Model.Contact?.Email"/>
    <div class="form-group">
        <label asp-for="@Model.Contact.FirstName">First Name</label>
        <input type="text" class="form-control" asp-for="@Model.Contact.FirstName">
    </div>
    <!-- ... other fields ... -->
    <button class="btn btn-primary">Submit</button>
</form>
```

The validation logic is defined within the `_EmailField.cshtml` partial. It uses htmx to trigger a validation check whenever the input changes.

**`_EmailField.cshtml`**
```razor
@model string
@{
    string? validationError = ViewData["InvalidEmailMessage"]?.ToString();
    bool hasError = !String.IsNullOrEmpty(validationError);
}
<div hx-target="this" hx-swap="outerHTML" class="@(hasError ? "val-error" : string.Empty)">
    <label class="control-label">Email Address</label>
    <input name="email"
           hx-post="/InlineValidation?handler=email"
           hx-indicator="#ind"
           value="@Model"
           class="form-control">
    <img id="ind" src="/img/bars.svg" class="htmx-indicator"/>
    @if (hasError)
    {
        <div class='error-message'>@validationError</div>
    }
</div>
```

**Key htmx attributes used:**
*   `hx-post`: Sends the current value of the email field to the `OnPostEmail` handler. By default, for inputs, this is triggered by the `change` event.
*   `hx-target="this"`: Ensures the server's response replaces the entire validation container (the wrapping `div`).
*   `hx-swap="outerHTML"`: Replaces the entire container so that the error classes and messages can be updated.
*   `hx-indicator="#ind"`: Shows a loading spinner while the server is validating the input.

#### The Backend: C# PageModel

On the server, the `IndexModel` handles both the individual field validation and the final form submission. The `OnPostEmail` handler performs the inline check and returns the partial view.

**`Index.cshtml.cs`**
```csharp
public class IndexModel : PageModel
{
    public string ExistingEmail { get; private set; } = "firstname.lastname@example.com";
    [ViewData] public string InvalidEmailMessage { get; set; }

    // This handler handles the inline htmx validation request
    public PartialViewResult OnPostEmail(string email)
    {
        if (String.IsNullOrEmpty(email) || !email.Contains('@'))
        {
            InvalidEmailMessage = "Please enter a valid email address";
        }
        else if (email == ExistingEmail)
        {
            InvalidEmailMessage = "That email is already taken. Please enter another email.";
        }

        // Return the same partial view with the validation message set in ViewData
        return Partial("_EmailField", email);
    }

    // Standard POST handler for the full form submission
    public IActionResult OnPost(Contact contact)
    {
        // Perform final validation check before processing
        if (contact.Email == ExistingEmail)
        {
            InvalidEmailMessage = "That email is already taken.";
            return Page();
        }

        // Process valid form submission...
        return Page();
    }
}
```

#### Why this works well

1.  **Immediate Feedback**: Users know instantly if their input is invalid or if a username/email is taken, reducing frustration at the end of a long form.
2.  **Logic Reuse**: The same validation logic can be used for both the inline htmx check and the final server-side form processing.
3.  **Clean Separation**: The validation UI (errors, styles) is encapsulated within a partial view, making the main form template easier to read.
4.  **No Custom JavaScript**: You get rich, asynchronous validation behavior using only standard ASP.NET Core tools and declarative htmx attributes.
