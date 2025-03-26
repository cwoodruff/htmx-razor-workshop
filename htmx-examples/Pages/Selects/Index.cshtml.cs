using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace htmx_examples.Pages.Selects;

public class IndexModel : PageModel
{
    private static readonly Dictionary<string, List<string>> MakeModel;

    static IndexModel()
    {
        IndexModel.MakeModel = new Dictionary<string, List<string>>();
        MakeModel.Add("Audi", new() { "A1", "A4", "A6" });
        MakeModel.Add("Toyota", new() { "Landcruiser", "Tacoma", "Yaris" });
        MakeModel.Add("BMW", new() { "325i", "325ix", "X5" });
    }

    public void OnGet()
    {
        ManufacturerMake = MakeModel.Keys.ToList();
        Make = ManufacturerMake.First();
        ManufacturerModels = MakeModel[Make];
    }

    public List<String> ManufacturerMake { get; set; }
    public List<String> ManufacturerModels { get; set; }
    [FromQuery(Name = "make")] public string Make { get; set; }

    public PartialViewResult OnGetModels()
    {
        ManufacturerModels = MakeModel[Make];

        return Partial("_modelSelector", ManufacturerModels);
    }
}