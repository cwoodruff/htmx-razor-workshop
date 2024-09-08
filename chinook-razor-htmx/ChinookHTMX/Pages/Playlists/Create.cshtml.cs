using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ChinookHTMX.Entities;
using Htmx;

namespace ChinookHTMX.Pages.Playlists;

public class CreateModel(Data.ChinookContext context) : PageModel
{
    [BindProperty] public Playlist Playlist { get; set; } = default!;

    public IActionResult OnGet()
    {
        if (Request.IsHtmx())
        {
            return Partial("Playlists/CreateModal", this);
        }

        return Page();
    }

    // To protect from overposting attacks, see https://aka.ms/RazorPagesCRUD
    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Partial("Playlists/CreateModal", Playlist);
        }

        context.Playlists.Add(Playlist);
        await context.SaveChangesAsync();
        return Partial("_CreateSuccess", this);
    }
}