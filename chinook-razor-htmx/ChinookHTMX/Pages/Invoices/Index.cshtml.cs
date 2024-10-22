using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ChinookHTMX.Entities;

namespace ChinookHTMX.Pages.Invoices;

public class IndexModel(Data.ChinookContext context) : PageModel
{
    public IList<Invoice> Invoices { get; set; } = default!;

    public async Task OnGetAsync()
    {
        Invoices = await context.Invoices
            .Include(i => i.Customer).ToListAsync();
    }
}