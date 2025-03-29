using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ChinookHTMX.Entities;
using Htmx;

namespace ChinookHTMX.Pages.Artists;

public class DetailsModel(Data.ChinookContext context) : PageModel
{
    public Artist Artist { get; set; } = default!;

    public async Task<IActionResult> OnGetAsync(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        Artist = await context.Artists.Include(a => a.Albums).FirstOrDefaultAsync(m => m.Id == id);

        if (Artist == null)
        {
            return NotFound();
        }

        if (Request.IsHtmx())
        {
            return Partial("Artists/DetailsModal", this);
        }

        return Page();
    }
}