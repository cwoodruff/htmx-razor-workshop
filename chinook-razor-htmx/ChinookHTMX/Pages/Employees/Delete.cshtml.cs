using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ChinookHTMX.Entities;

namespace ChinookHTMX.Pages.Employees;

public class DeleteModel(Data.ChinookContext context) : PageModel
{
    [BindProperty] public Employee Employee { get; set; } = default!;

    public async Task<IActionResult> OnGetAsync(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var employee = await context.Employees.FirstOrDefaultAsync(m => m.Id == id);

        if (employee == null)
        {
            return NotFound();
        }
        else
        {
            Employee = employee;
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var employee = await context.Employees.FindAsync(id);
        if (employee != null)
        {
            Employee = employee;
            context.Employees.Remove(Employee);
            await context.SaveChangesAsync();
        }

        return RedirectToPage("./Index");
    }
}