using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ChinookHTMX.Entities;

namespace ChinookHTMX.Pages.Tracks;

public class EditModel(Data.ChinookContext context) : PageModel
{
    [BindProperty] public Track Track { get; set; } = default!;

    public async Task<IActionResult> OnGetAsync(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var track = await context.Tracks.FirstOrDefaultAsync(m => m.Id == id);
        if (track == null)
        {
            return NotFound();
        }

        Track = track;
        ViewData["AlbumId"] = new SelectList(context.Albums, "Id", "Id");
        ViewData["GenreId"] = new SelectList(context.Genres, "Id", "Id");
        ViewData["MediaTypeId"] = new SelectList(context.MediaTypes, "Id", "Id");
        return Page();
    }

    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see https://aka.ms/RazorPagesCRUD.
    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        context.Attach(Track).State = EntityState.Modified;

        try
        {
            await context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!TrackExists(Track.Id))
            {
                return NotFound();
            }
            else
            {
                throw;
            }
        }

        return RedirectToPage("./Index");
    }

    private bool TrackExists(int id)
    {
        return context.Tracks.Any(e => e.Id == id);
    }
}