using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ChinookHTMX.Entities;
using Htmx;

namespace ChinookHTMX.Pages.Artists;

public class CreateModel(Data.ChinookContext context) : PageModel
{
    [BindProperty] public Artist Artist { get; set; } = default!;

    public IActionResult OnGet()
    {
        if (Request.IsHtmx())
        {
            return Partial("Artists/CreateModal", this);
        }

        return Page();
    }

    // To protect from overposting attacks, see https://aka.ms/RazorPagesCRUD
    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Partial("Artists/CreateModal", Artist);
        }

        context.Artists.Add(Artist);
        await context.SaveChangesAsync();
        return Partial("_CreateSuccess", this);
    }
}