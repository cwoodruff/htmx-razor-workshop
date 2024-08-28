using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ChinookHTMX.Entities;

namespace ChinookHTMX.Pages.Albums;

public class IndexModel(Data.ChinookContext context) : PageModel
{
    public IList<Album> Album { get; set; } = default!;

    public async Task OnGetAsync()
    {
        Album = await context.Albums
            .Include(a => a.Artist).ToListAsync();
    }
}