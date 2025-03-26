using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace htmx_examples.Pages.BulkUpdate;

public class Index : PageModel
{
    private readonly IContactService service;

    public Index(IContactService service)
    {
        this.service = service;
    }

    public List<Contact>? ContactTableRows { get; set; }

    public void OnGet()
    {
        ContactTableRows = service.Get().ToList();
    }


    public PartialViewResult OnPutActivate(int[] Ids)
    {
        foreach (var Id in Ids)
            service.Update(Id, true);
        var models = service.Get();
        foreach (var m in models)
            if (Ids.Contains(m.Id))
                m.Updated = true;
            else m.Updated = false;
        return Partial("_tbody", models.ToList());
    }

    public PartialViewResult OnPutDeactivate(int[] Ids)
    {
        foreach (var Id in Ids)
            service.Update(Id, false);
        var models = service.Get();
        foreach (var m in models)
            if (Ids.Contains(m.Id))
                m.Updated = true;
            else m.Updated = false;

        return Partial("_tbody", models.ToList());
    }
}