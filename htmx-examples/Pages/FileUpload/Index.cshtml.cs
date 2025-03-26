using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace htmx_examples.Pages.FileUpload;

public class IndexModel : PageModel
{
    public void OnGet()
    {
    }

    [BindProperty, Display(Name = "File")] public IFormFile UploadedFile { get; set; }

    public PartialViewResult OnPostUpload()
    {
        Task.Delay(1200);

        return Partial("_javascript", UploadedFile);
    }

    public PartialViewResult OnPostUpload2()
    {
        Task.Delay(1200);
        return Partial("_hyperscript", UploadedFile);
    }
}