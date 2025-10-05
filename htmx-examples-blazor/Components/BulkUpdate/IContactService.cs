using htmx_examples_blazor.Pages.BulkUpdate;

namespace htmx_examples_blazor.Pages.BulkUpdate;

public interface IContactService
{
    IEnumerable<Contact> Get();
    void Update(int Id, bool Status);
}