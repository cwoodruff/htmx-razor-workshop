using htmx_examples_blazor.Pages.BulkUpdate;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace htmx_examples_blazor.Components.BulkUpdate;

public class BulkUpdateEndpoints : IEndpoints
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        app.MapPut("/BulkUpdate/Activate", (IContactService service, [FromForm] int[] Ids) =>
        {
            foreach (var Id in Ids)
                service.Update(Id, true);
            var models = service.Get();
            foreach (var m in models)
                if (Ids.Contains(m.Id))
                    m.Updated = true;
                else m.Updated = false;

            return new RazorComponentResult<TableBody>(new
            {
                Model = models.ToList()
            });
        });

        app.MapPut("/BulkUpdate/Deactivate", (IContactService service, [FromForm] int[] Ids) =>
        {
            foreach (var Id in Ids)
                service.Update(Id, false);
            var models = service.Get();
            foreach (var m in models)
                if (Ids.Contains(m.Id))
                    m.Updated = true;
                else m.Updated = false;

            return new RazorComponentResult<TableBody>(new
            {
                Model = models.ToList()
            });
        });
   }
}
