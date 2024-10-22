using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ChinookHTMX.Entities;

namespace ChinookHTMX.Pages.Customers;

public class IndexModel(Data.ChinookContext context) : PageModel
{
    public IList<Customer> Customer { get; set; } = default!;

    public async Task OnGetAsync()
    {
        Customer = await context.Customers
            .Include(c => c.SupportRep)
            .Include(c => c.Invoices)
            .ToListAsync();
    }
}