using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ChinookHTMX.Entities;

namespace ChinookHTMX.Pages.Playlists;

public class IndexModel(Data.ChinookContext context) : PageModel
{
    public IList<Playlist> Playlist { get; set; } = default!;

    public async Task OnGetAsync()
    {
        Playlist = await context.Playlists
            .Include(p => p.Tracks)
            .ToListAsync();
    }
}