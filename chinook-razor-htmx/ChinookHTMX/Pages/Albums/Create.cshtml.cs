using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using ChinookHTMX.Entities;
using Htmx;

namespace ChinookHTMX.Pages.Albums;

public class CreateModel(Data.ChinookContext context) : PageModel
{
    [BindProperty] public Album Album { get; set; } = default!;

    public IActionResult OnGet()
    {
        if (Request.IsHtmx())
        {
            return Partial("Albums/CreateModal", this);
        }

        return Page();
    }

    // To protect from overposting attacks, see https://aka.ms/RazorPagesCRUD
    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Partial("Albums/CreateModal", Album);
        }

        context.Albums.Add(Album);
        await context.SaveChangesAsync();
        return Partial("_CreateSuccess", this);
    }
}