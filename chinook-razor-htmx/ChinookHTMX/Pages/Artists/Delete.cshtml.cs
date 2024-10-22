using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ChinookHTMX.Entities;
using Htmx;

namespace ChinookHTMX.Pages.Artists;

public class DeleteModel(Data.ChinookContext context) : PageModel
{
    [BindProperty] public Artist Artist { get; set; } = default!;

    public async Task<IActionResult> OnGetAsync(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        Artist = await context.Artists.FirstOrDefaultAsync(m => m.Id == id);

        if (Artist == null)
        {
            return NotFound();
        }

        if (Request.IsHtmx())
        {
            return Partial("Artists/DeleteModal", this);
        }

        return Page();
    }
    
    public async Task<IActionResult> OnPostAsync(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var artist = await context.Artists.FindAsync(id);
        if (artist != null)
        {
            Artist = artist;
            context.Artists.Remove(Artist);
            await context.SaveChangesAsync();
        }

        return Partial("_DeleteSuccess", this);
    }
}