using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ChinookHTMX.Entities;
using Htmx;

namespace ChinookHTMX.Pages.MediaTypes;

public class DetailsModel(Data.ChinookContext context) : PageModel
{
    public MediaType MediaType { get; set; } = default!;

    public async Task<IActionResult> OnGetAsync(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        MediaType = await context.MediaTypes.FirstOrDefaultAsync(m => m.Id == id);

        if (MediaType == null)
        {
            return NotFound();
        }

        if (Request.IsHtmx())
        {
            return Partial("MediaTypes/DetailsModal", this);
        }

        return Page();
    }
}