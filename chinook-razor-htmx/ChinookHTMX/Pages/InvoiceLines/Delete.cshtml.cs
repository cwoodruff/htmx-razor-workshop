using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ChinookHTMX.Entities;

namespace ChinookHTMX.Pages.InvoiceLines;

public class DeleteModel(Data.ChinookContext context) : PageModel
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
        else
        {
            InvoiceLine = invoiceline;
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var invoiceline = await context.InvoiceLines.FindAsync(id);
        if (invoiceline != null)
        {
            InvoiceLine = invoiceline;
            context.InvoiceLines.Remove(InvoiceLine);
            await context.SaveChangesAsync();
        }

        return RedirectToPage("./Index");
    }
}