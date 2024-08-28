using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using ChinookHTMX.Entities;

namespace ChinookHTMX.Pages.InvoiceLines;

public class CreateModel(Data.ChinookContext context) : PageModel
{
    public IActionResult OnGet()
    {
        ViewData["InvoiceId"] = new SelectList(context.Invoices, "Id", "Id");
        ViewData["TrackId"] = new SelectList(context.Tracks, "Id", "Id");
        return Page();
    }

    [BindProperty] public InvoiceLine InvoiceLine { get; set; } = default!;

    // To protect from overposting attacks, see https://aka.ms/RazorPagesCRUD
    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        context.InvoiceLines.Add(InvoiceLine);
        await context.SaveChangesAsync();

        return RedirectToPage("./Index");
    }
}