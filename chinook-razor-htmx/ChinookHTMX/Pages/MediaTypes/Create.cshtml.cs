using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ChinookHTMX.Entities;
using Htmx;

namespace ChinookHTMX.Pages.MediaTypes;

public class CreateModel(Data.ChinookContext context) : PageModel
{ 
    [BindProperty] public MediaType MediaType { get; set; } = default!;

    public IActionResult OnGet()
    {
        if (Request.IsHtmx())
        {
            return Partial("MediaTypes/CreateModal", this);
        }

        return Page();
    }

    // To protect from overposting attacks, see https://aka.ms/RazorPagesCRUD
    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Partial("MediaTypes/CreateModal", MediaType);
        }

        context.MediaTypes.Add(MediaType);
        await context.SaveChangesAsync();
        return Partial("_CreateSuccess", this);
    }
}