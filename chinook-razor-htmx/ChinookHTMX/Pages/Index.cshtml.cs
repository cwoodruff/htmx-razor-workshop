using ChinookHTMX.Entities;
using Htmx;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ChinookHTMX.Pages;

public class IndexModel(Data.ChinookContext context, ILogger<IndexModel> logger) : PageModel
{
    public IList<Invoice> Invoices { get; set; } = default!;
    
    [BindProperty(SupportsGet = true)]
    public int Id { get; set; }

    public async Task OnGetAsync()
    {
        Invoices = await context.Invoices
            .Include(i => i.Customer).ToListAsync();
    }
    
    public async Task<PartialViewResult> OnGetRow()
    {
        return Partial("_InvoiceRow", await context.Invoices.Include(i => i.Customer).FirstOrDefaultAsync(i => i.Id == Id));
    }

    public IActionResult OnGetEdit()
    {
        return Partial("_InvoiceEdit", context.Invoices.Include(i => i.Customer).FirstOrDefault(i => i.Id ==  Id));
    }

    public async Task<IActionResult> OnPostUpdate([FromForm] Invoice invoice)
    {
        if (await context.Invoices.Include(i => i.Customer).FirstOrDefaultAsync(x => x.Id == Id) is { } i)
        {
            i.BillingAddress = invoice.BillingAddress;
            i.BillingCity = invoice.BillingCity;
            i.BillingState = invoice.BillingState;
            i.BillingCountry = invoice.BillingCountry;
            i.BillingPostalCode = invoice.BillingPostalCode;
            i.InvoiceDate = invoice.InvoiceDate;

            return Request.IsHtmx()
                ? Partial("_InvoiceRow", i)
                : Redirect("Index");
        }
        return BadRequest();
    }
}