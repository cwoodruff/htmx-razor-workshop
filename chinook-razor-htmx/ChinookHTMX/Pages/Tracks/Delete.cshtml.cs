using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ChinookHTMX.Entities;

namespace ChinookHTMX.Pages.Tracks;

public class DeleteModel(Data.ChinookContext context) : PageModel
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
        else
        {
            Track = track;
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var track = await context.Tracks.FindAsync(id);
        if (track != null)
        {
            Track = track;
            context.Tracks.Remove(Track);
            await context.SaveChangesAsync();
        }

        return RedirectToPage("./Index");
    }
}