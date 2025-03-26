using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace htmx_examples.Pages.DeleteRow;

public class IndexModel : PageModel
{
    IContactService contactService;
    readonly IAntiforgery _antiforgery;

    public string? RequestToken { get; set; }
    public IList<Contact>? Contacts { get; set; }

    [FromQuery(Name = "Id")] public int Id { get; set; }

    public IndexModel(IContactService contactService, IAntiforgery antiforgery)
    {
        this.contactService = contactService;
        _antiforgery = antiforgery;
    }

    public void OnGet()
    {
        this.Contacts = contactService.Get().ToArray();
        var tokenSet = _antiforgery.GetAndStoreTokens(HttpContext);
        RequestToken = tokenSet.RequestToken;
    }

    public IActionResult OnPostContact()
    {
        //contactService.Delete(this.Id);
        return new OkResult();
    }

    public IActionResult OnDeleteContact()
    {
        contactService.Delete(this.Id);
        return new OkResult();
    }
}