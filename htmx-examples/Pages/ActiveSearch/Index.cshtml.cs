using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace htmx_examples.Pages.ActiveSearch;

public class IndexModel : PageModel
{
    private readonly HttpClient _httpClient;
    readonly IAntiforgery _antiforgery;
    public string? RequestToken { get; set; }

    public IndexModel(IHttpClientFactory factory, IAntiforgery antiforgery)
    {
        _httpClient = factory.CreateClient();
        _antiforgery = antiforgery;
    }

    public void OnGet()
    {
        var tokenSet = _antiforgery.GetAndStoreTokens(HttpContext);

        RequestToken = tokenSet.RequestToken;
        ;
    }

    [BindProperty] public string SearchText { get; set; }
    public List<Country> Countries { get; set; }

    public async Task<PartialViewResult> OnPostSearch()
    {
        Countries = new();
        var result = await _httpClient.GetStringAsync($"https://restcountries.com/v3.1/name/{SearchText}");
        //var jsonstr = await result.Content.ReadAsStringAsync();
        var json = JsonArray.Parse(result);
        foreach (var country in json.AsArray())
        {
            this.Countries.Add(new(country["name"]["common"].ToString()));
        }

        return Partial("_searchResult", Countries);
    }
}