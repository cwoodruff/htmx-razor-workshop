using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ChinookHTMX.Entities;
using Htmx;

namespace ChinookHTMX.Pages.Genres;

public class CreateModel(Data.ChinookContext context) : PageModel
{
    [BindProperty] public Genre Genre { get; set; } = default!;

    public IActionResult OnGet()
    {
        if (Request.IsHtmx())
        {
            return Partial("Genres/CreateModal", this);
        }

        return Page();
    }

    // To protect from overposting attacks, see https://aka.ms/RazorPagesCRUD
    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Partial("Genres/CreateModal", Genre);
        }

        context.Genres.Add(Genre);
        await context.SaveChangesAsync();
        return Partial("_CreateSuccess", this);
    }
}