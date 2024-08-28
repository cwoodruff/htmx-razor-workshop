using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ChinookHTMX.Entities;

namespace ChinookHTMX.Pages.Artists;

public class IndexModel(Data.ChinookContext context) : PageModel
{
    public IList<Artist> Artists { get; set; } = default!;

    public async Task OnGetAsync()
    {
        Artists = await context.Artists.ToListAsync();
    }
}