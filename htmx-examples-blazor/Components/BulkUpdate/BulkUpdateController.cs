using htmx_examples_blazor.Pages.BulkUpdate;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace htmx_examples_blazor.Components.BulkUpdate
{
    [ApiController]
    [RequireAntiforgeryToken]
    public class BulkUpdateController : ControllerBase
    {
        private readonly IContactService service;

        public BulkUpdateController(IContactService service)
        {
            this.service = service;
        }

        [HttpPut("/BulkUpdate/Activate")]
        public RazorComponentResult OnPutActivate([FromForm]int[] Ids)
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
        }

        [HttpPut("/BulkUpdate/Deactivate")]
        public RazorComponentResult OnPutDeactivate([FromForm]int[] Ids)
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
        }
    }
}
