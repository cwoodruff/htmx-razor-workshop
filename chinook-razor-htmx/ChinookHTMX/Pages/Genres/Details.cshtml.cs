using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ChinookHTMX.Entities;
using Htmx;

namespace ChinookHTMX.Pages.Genres;

public class DetailsModel(Data.ChinookContext context) : PageModel
{
    public Genre Genre { get; set; } = default!;

    public async Task<IActionResult> OnGetAsync(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        Genre = await context.Genres.FirstOrDefaultAsync(m => m.Id == id);

        if (Genre == null)
        {
            return NotFound();
        }

        if (Request.IsHtmx())
        {
            return Partial("Genres/DetailsModal", this);
        }

        return Page();
    }
}