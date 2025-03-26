using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace htmx_examples.Pages.InlineValidation;

public class IndexModel : PageModel
{
    [BindProperty] public Contact? Contact { get; private set; }

    public string ExistingEmail { get; private set; } = "firstname.lastname@example.com";
    [ViewData] public string InvalidEmailMessage { get; set; }

    public void OnGet()
    {
        Contact = new Contact("First", "Last", "name@example.com");
    }

    public IActionResult OnPost(Contact contact)
    {
        // This method runs when a POST request is made to the page.
        if (ModelState.IsValid)
        {
            Contact = contact;
            return Page();
        }

        if (String.IsNullOrEmpty(contact.Email) || !contact.Email.Contains('@'))
        {
            InvalidEmailMessage = "Please enter a valid email address";
        }
        else if (contact.Email == ExistingEmail)
        {
            InvalidEmailMessage = "That email is already taken. Please enter another email.";
        }

        return Page();
    }

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

        return Partial("_EmailField", email);
    }
}