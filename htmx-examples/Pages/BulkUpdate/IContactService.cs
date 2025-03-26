namespace htmx_examples.Pages.BulkUpdate;

public interface IContactService
{
    IEnumerable<Contact> Get();
    void Update(int Id, bool Status);
}