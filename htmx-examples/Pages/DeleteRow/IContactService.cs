namespace htmx_examples.Pages.DeleteRow;

public interface IContactService
{
    IEnumerable<Contact> Get();
    void Delete(int Id);
}