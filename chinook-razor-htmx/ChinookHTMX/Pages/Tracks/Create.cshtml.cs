using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using ChinookHTMX.Entities;

namespace ChinookHTMX.Pages.Tracks;

public class CreateModel(Data.ChinookContext context) : PageModel
{
    public IActionResult OnGet()
    {
        ViewData["AlbumId"] = new SelectList(context.Albums, "Id", "Id");
        ViewData["GenreId"] = new SelectList(context.Genres, "Id", "Id");
        ViewData["MediaTypeId"] = new SelectList(context.MediaTypes, "Id", "Id");
        return Page();
    }

    [BindProperty] public Track Track { get; set; } = default!;

    // To protect from overposting attacks, see https://aka.ms/RazorPagesCRUD
    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        context.Tracks.Add(Track);
        await context.SaveChangesAsync();

        return RedirectToPage("./Index");
    }
}