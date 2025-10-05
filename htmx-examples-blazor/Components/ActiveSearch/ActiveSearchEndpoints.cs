using System;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace htmx_examples_blazor.Components.ActiveSearch;

public class ActiveSearchEndpoints : IEndpoints
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        app.MapPost("/ActiveSearch/Search", async (HttpClient httpClient, [FromForm] string searchText) =>
        {
            var result = await httpClient.GetStringAsync($"https://restcountries.com/v3.1/name/{searchText}");
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
        });
    }
}
