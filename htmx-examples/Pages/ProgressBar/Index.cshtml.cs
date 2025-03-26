using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace htmx_examples.Pages.ProgressBar;

public class IndexModel : PageModel
{
    public static int percent { get; set; } = 0;
    readonly IAntiforgery _antiforgery;

    public string? RequestToken { get; set; }

    public IndexModel(IAntiforgery antiforgery)
    {
        _antiforgery = antiforgery;
    }

    public void OnGet()
    {
        var tokenSet = _antiforgery.GetAndStoreTokens(HttpContext);
        RequestToken = tokenSet.RequestToken;
    }

    public void OnPost()
    {
    }

    [ViewData] public string PercentDone { get; set; }
    [ViewData] public string Status { get; set; }

    public PartialViewResult OnGetJobStatus()
    {
        percent = percent switch
        {
            0 => 2,
            2 => 18,
            18 => 22,
            22 => 52,
            52 => 67,
            67 => 98,
            98 => 100,
            _ => 0
        };
        if (percent >= 100)
            HttpContext.Response.Headers["HX-Trigger"] = "done";
        PercentDone = percent.ToString();
        return Partial("_ProgressBar");
    }

    public PartialViewResult OnGetFinalizeJob()
    {
        percent = 0;
        PercentDone = "100";
        Status = "Complete";
        return Partial("_Progress");
    }


    public PartialViewResult OnPostStartJob()
    {
        if (percent == 0)
            percent = 2;
        Status = "Running";
        PercentDone = percent.ToString();
        return Partial("_Progress");
    }
}