using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ChinookHTMX.Entities;
using Htmx;

namespace ChinookHTMX.Pages.Customers;

public class DetailsModel(Data.ChinookContext context) : PageModel
{
    public Customer? Customer { get; set; } = default!;

    public async Task<IActionResult> OnGetAsync(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        Customer = await context.Customers
            .Include(c => c.Invoices)
            .Include(c => c.SupportRep)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (Customer == null)
        {
            return NotFound();
        }

        if (Request.IsHtmx())
        {
            return Partial("Customers/DetailsModal", this);
        }

        return Page();
    }
}