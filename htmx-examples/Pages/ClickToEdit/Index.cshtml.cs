using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace htmx_examples.Pages.ClickToEdit;

public class Index : PageModel
{
    private IContactService contactService;
    public Contact Contact { get; private set; }

    public Index(IContactService service)
    {
        contactService = service;
        Contact = contactService.Get(1);
    }

    public void OnGet()
    {
    }

    public IActionResult OnPost()
    {
        // This method runs when a POST request is made to the page.

        return Page();
    }

    public IActionResult OnPutReplaceContact(Contact model)
    {
        // This method runs when a POST request is made to the page.
        contactService.Update(1, model);

        return Partial("_DisplayContactForm", model);
    }

    public PartialViewResult OnGetEditContact()
    {
        Contact = contactService.Get(1);

        return Partial("_EditContactForm", Contact);
    }

    public PartialViewResult OnGetDisplayContact()
    {
        var contact = contactService.Get(1);

        return Partial("_DisplayContactForm", contact);
    }
}