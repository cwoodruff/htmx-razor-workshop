using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ChinookHTMX.Entities;
using Htmx;

namespace ChinookHTMX.Pages.Albums;

public class DetailsModel(Data.ChinookContext context) : PageModel
{
    public Album Album { get; set; } = default!;

    public async Task<IActionResult> OnGetAsync(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        Album = await context.Albums.FirstOrDefaultAsync(m => m.Id == id);

        if (Album == null)
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