using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ChinookHTMX.Entities;

namespace ChinookHTMX.Pages.MediaTypes;

public class IndexModel(Data.ChinookContext context) : PageModel
{
    public IList<MediaType> MediaType { get; set; } = default!;

    public async Task OnGetAsync()
    {
        MediaType = await context.MediaTypes.ToListAsync();
    }
}