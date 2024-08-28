using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ChinookHTMX.Entities;

namespace ChinookHTMX.Pages.Invoices;

public class DeleteModel(Data.ChinookContext context) : PageModel
{
    [BindProperty] public Invoice Invoice { get; set; } = default!;

    public async Task<IActionResult> OnGetAsync(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var invoice = await context.Invoices.FirstOrDefaultAsync(m => m.Id == id);

        if (invoice == null)
        {
            return NotFound();
        }
        else
        {
            Invoice = invoice;
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var invoice = await context.Invoices.FindAsync(id);
        if (invoice != null)
        {
            Invoice = invoice;
            context.Invoices.Remove(Invoice);
            await context.SaveChangesAsync();
        }

        return RedirectToPage("./Index");
    }
}