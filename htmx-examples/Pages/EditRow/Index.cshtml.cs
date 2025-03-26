using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace htmx_examples.Pages.EditRow;

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

    public PartialViewResult OnGetEdit(int Id)
    {
        var contact = contactService.Get(Id);

        return Partial("_EditRow", contact);
    }

    public PartialViewResult OnGetView(int Id)
    {
        var contact = contactService.Get(Id);

        return Partial("_TableRow", contact);
    }

    public PartialViewResult OnPut(Contact contact)
    {
        contactService.Update(contact);

        return Partial("_TableRow", contact);
    }
}