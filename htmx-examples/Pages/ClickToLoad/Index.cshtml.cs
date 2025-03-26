using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace htmx_examples.Pages.ClickToLoad;

public class IndexModel : PageModel
{
    [ViewData] public int PageCount { get; set; } = 5;
    [ViewData] public int PageNumber { get; set; } = 0;
    [FromQuery(Name = "page")] public int NextPage { get; set; }
    public List<Contact>? Contacts { get; set; }

    public void OnGet()
    {
        this.Contacts = GetPagedResults(PageNumber, PageCount).ToList();
    }

    private IEnumerable<Contact> GetPagedResults(int page, int take)
    {
        var start = 10 + (page * take);
        for (int i = start; i < start + take; i++)
        {
            yield return new Contact("Woody", $"me{i}@woody.dev", Guid.NewGuid());
        }
    }

    public PartialViewResult OnGetNextPage()
    {
        PageNumber = NextPage;
        return Partial("_ClickToLoadButton", GetPagedResults(NextPage, PageCount).ToList());
    }
}