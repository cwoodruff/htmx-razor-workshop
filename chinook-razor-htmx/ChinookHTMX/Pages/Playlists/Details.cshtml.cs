using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ChinookHTMX.Entities;
using Htmx;

namespace ChinookHTMX.Pages.Playlists;

public class DetailsModel(Data.ChinookContext context) : PageModel
{
    public Playlist Playlist { get; set; } = default!;

    public async Task<IActionResult> OnGetAsync(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        Playlist = await context.Playlists.FirstOrDefaultAsync(m => m.Id == id);

        if (Playlist == null)
        {
            return NotFound();
        }

        if (Request.IsHtmx())
        {
            return Partial("Playlists/DetailsModal", this);
        }

        return Page();
    }
}