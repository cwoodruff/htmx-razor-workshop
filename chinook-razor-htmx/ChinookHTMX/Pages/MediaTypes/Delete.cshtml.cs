using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ChinookHTMX.Entities;
using Htmx;

namespace ChinookHTMX.Pages.MediaTypes;

public class DeleteModel(Data.ChinookContext context) : PageModel
{
    [BindProperty] public MediaType MediaType { get; set; } = default!;

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
            return Partial("MediaTypes/DeleteModal", this);
        }

        return Page();
    }
    
    public async Task<IActionResult> OnPostAsync(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var mediaType = await context.MediaTypes.FindAsync(id);
        if (mediaType != null)
        {
            MediaType = mediaType;
            context.MediaTypes.Remove(MediaType);
            await context.SaveChangesAsync();
        }

        return Partial("_DeleteSuccess", this);
    }
}