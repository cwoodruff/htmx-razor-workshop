using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ChinookHTMX.Entities;
using Htmx;

namespace ChinookHTMX.Pages.Artists;

public class EditModel(Data.ChinookContext context) : PageModel
{
    [BindProperty] public Artist Artist { get; set; } = default!;

    public async Task<IActionResult> OnGet(int? id)
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
            return Partial("EditModal", this);
        }

        return Page();
    }

    // To protect from overposting attacks, see https://aka.ms/RazorPagesCRUD
    public async Task<IActionResult> OnPostModalAsync()
    {
        if (!ModelState.IsValid)
        {
            return Partial("Artists/EditModal", this);
        }

        context.Attach(Artist).State = EntityState.Modified;
        try
        {
            await context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!ArtistExists(Artist.Id))
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

    private bool ArtistExists(int id)
    {
        return context.Artists.Any(e => e.Id == id);
    }
}