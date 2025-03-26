namespace htmx_examples.Pages.DeleteRow;

public class ContactService : IContactService
{
    private List<Contact> contacts;

    public ContactService()
    {
        int key = 0;
        // Initialize the static contact member.
        contacts = new();
        contacts.Add(new(++key, "Scarlett Nolan", "scarlett.nolan@example.com"));
        contacts.Add(new(++key, "Leonardo Evans", "leonardo.evans@example.com"));
        contacts.Add(new(++key, "Natalie Damon", "natalie.damon@example.com"));
        contacts.Add(new(++key, "Chris Johansson", "chris.johansson@example.com") { Status = false });
    }

    public void Delete(int Id)
    {
        contacts.RemoveAll(x => x.Id == Id);
    }

    public IEnumerable<Contact> Get()
    {
        return contacts;
    }
}