using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ChinookHTMX.Entities;
using Htmx;

namespace ChinookHTMX.Pages.Customers;

public class DeleteModel(Data.ChinookContext context) : PageModel
{
    [BindProperty] public Customer Customer { get; set; } = default!;

    public async Task<IActionResult> OnGetAsync(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        Customer = await context.Customers.FirstOrDefaultAsync(m => m.Id == id);

        if (Customer == null)
        {
            return NotFound();
        }

        if (Request.IsHtmx())
        {
            return Partial("Customers/DeleteModal", this);
        }

        return Page();
    }
    
    public async Task<IActionResult> OnPostAsync(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var customer = await context.Customers.FindAsync(id);
        if (customer != null)
        {
            Customer = customer;
            context.Customers.Remove(Customer);
            await context.SaveChangesAsync();
        }

        return Partial("_DeleteSuccess", this);
    }
}