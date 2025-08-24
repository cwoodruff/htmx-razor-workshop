using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace htmx_examples.Components.ActiveSearch;

[ApiController]
[ValidateAntiForgeryToken]
public class ActiveSearchController : ControllerBase
{
    private readonly HttpClient _httpClient;

    public ActiveSearchController(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    [HttpPost("/ActiveSearch/Search")]
    public async Task<IResult> SearchCountries([FromForm]string searchText)
    {
        var result = await _httpClient.GetStringAsync($"https://restcountries.com/v3.1/name/{searchText}");
        var model = new List<Country>();
        var json = JsonArray.Parse(result);
        foreach (var country in json.AsArray())
        {
            model.Add(new Country(country["name"]["common"].ToString()));
        }

        return new RazorComponentResult<SearchResult>(new
        {
            model
        });
    }
}