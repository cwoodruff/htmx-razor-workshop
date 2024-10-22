using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ChinookHTMX.Entities;
using Htmx;

namespace ChinookHTMX.Pages.Playlists;

public class DeleteModel(Data.ChinookContext context) : PageModel
{
    [BindProperty] public Playlist Playlist { get; set; } = default!;

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
            return Partial("Playlists/DeleteModal", this);
        }

        return Page();
    }
    
    public async Task<IActionResult> OnPostAsync(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var playlist = await context.Playlists.FindAsync(id);
        if (playlist != null)
        {
            Playlist = playlist;
            context.Playlists.Remove(Playlist);
            await context.SaveChangesAsync();
        }

        return Partial("_DeleteSuccess", this);
    }
}