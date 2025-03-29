using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ChinookHTMX.Entities;

namespace ChinookHTMX.Pages.InvoiceLines;

public class IndexModel(Data.ChinookContext context) : PageModel
{
    public IList<InvoiceLine> InvoiceLine { get; set; } = default!;

    public async Task OnGetAsync()
    {
        InvoiceLine = await context.InvoiceLines
            .Include(i => i.Invoice)
            .Include(i => i.Track).ToListAsync();
    }
}