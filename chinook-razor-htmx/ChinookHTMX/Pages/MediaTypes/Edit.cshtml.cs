using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ChinookHTMX.Entities;
using Htmx;

namespace ChinookHTMX.Pages.MediaTypes;

public class EditModel(Data.ChinookContext context) : PageModel
{
    [BindProperty] public MediaType MediaType { get; set; } = default!;

    public async Task<IActionResult> OnGet(int? id)
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
            return Partial("EditModal", this);
        }

        return Page();
    }

    // To protect from overposting attacks, see https://aka.ms/RazorPagesCRUD
    public async Task<IActionResult> OnPostModalAsync()
    {
        if (!ModelState.IsValid)
        {
            return Partial("MediaTypes/EditModal", this);
        }

        context.Attach(MediaType).State = EntityState.Modified;
        try
        {
            await context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!MediaTypeExists(MediaType.Id))
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

    private bool MediaTypeExists(int id)
    {
        return context.MediaTypes.Any(e => e.Id == id);
    }
}