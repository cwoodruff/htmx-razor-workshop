using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ChinookHTMX.Entities;

namespace ChinookHTMX.Pages.Tracks;

public class IndexModel(Data.ChinookContext context) : PageModel
{
    public IList<Track> Track { get; set; } = default!;

    public async Task OnGetAsync()
    {
        Track = await context.Tracks
            .Include(t => t.Album)
            .Include(t => t.Genre)
            .Include(t => t.MediaType).ToListAsync();
    }
}