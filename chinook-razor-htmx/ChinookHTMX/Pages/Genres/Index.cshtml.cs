using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ChinookHTMX.Entities;

namespace ChinookHTMX.Pages.Genres;

public class IndexModel(Data.ChinookContext context) : PageModel
{
    public IList<Genre> Genre { get; set; } = default!;

    public async Task OnGetAsync()
    {
        Genre = await context.Genres.ToListAsync();
    }
}