using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ChinookHTMX.Entities;
using Htmx;

namespace ChinookHTMX.Pages.Playlists;

public class EditModel(Data.ChinookContext context) : PageModel
{
    [BindProperty] public Playlist Playlist { get; set; } = default!;

    public async Task<IActionResult> OnGet(int? id)
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
            return Partial("EditModal", this);
        }

        return Page();
    }

    // To protect from overposting attacks, see https://aka.ms/RazorPagesCRUD
    public async Task<IActionResult> OnPostModalAsync()
    {
        if (!ModelState.IsValid)
        {
            return Partial("Playlists/EditModal", this);
        }

        context.Attach(Playlist).State = EntityState.Modified;
        try
        {
            await context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!PlaylistExists(Playlist.Id))
            {
                return NotFound();
            }
            else
            {
                throw;
            }
        }

        return Partial("_EditSuccess", this);
    }

    private bool PlaylistExists(int id)
    {
        return context.Playlists.Any(e => e.Id == id);
    }
}