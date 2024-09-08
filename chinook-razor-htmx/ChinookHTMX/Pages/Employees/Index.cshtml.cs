using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ChinookHTMX.Entities;

namespace ChinookHTMX.Pages.Employees;

public class IndexModel(Data.ChinookContext context) : PageModel
{
    public IList<Employee> Employee { get; set; } = default!;

    public async Task OnGetAsync()
    {
        Employee = await context.Employees
            .Include(e => e.ReportsToNavigation).ToListAsync();
    }
}