using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace htmx_examples.Pages.LazyLoading;

public class IndexModel : PageModel
{
    public IndexModel()
    {
    }

    public void OnGet()
    {
    }

    public IActionResult OnGetGraph()
    {
        return Content($"<img alt=\"Tokyo Climate\" src=\"https://htmx.org/img/tokyo.png\">");
    }
}