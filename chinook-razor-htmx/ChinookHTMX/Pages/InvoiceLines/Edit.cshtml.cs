using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ChinookHTMX.Entities;

namespace ChinookHTMX.Pages.InvoiceLines;

public class EditModel(Data.ChinookContext context) : PageModel
{
    [BindProperty] public InvoiceLine InvoiceLine { get; set; } = default!;

    public async Task<IActionResult> OnGetAsync(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var invoiceline = await context.InvoiceLines.FirstOrDefaultAsync(m => m.Id == id);
        if (invoiceline == null)
        {
            return NotFound();
        }

        InvoiceLine = invoiceline;
        ViewData["InvoiceId"] = new SelectList(context.Invoices, "Id", "Id");
        ViewData["TrackId"] = new SelectList(context.Tracks, "Id", "Id");
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

        context.Attach(InvoiceLine).State = EntityState.Modified;

        try
        {
            await context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!InvoiceLineExists(InvoiceLine.Id))
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

    private bool InvoiceLineExists(int id)
    {
        return context.InvoiceLines.Any(e => e.Id == id);
    }
}