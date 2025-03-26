namespace htmx_examples.Pages.EditRow;

public class ContactService : IContactService
{
    private List<Contact> contacts;

    public ContactService()
    {
        int key = 0;
        // Initialize the static contact member.
        contacts = new();
        contacts.Add(new(++key, "Scarlett Nolan", "scarlett.nolan@example.com"));
        contacts.Add(new(++key, "Leonardo Evans", "leonardo.evans   @example.com"));
        contacts.Add(new(++key, "Natalie Damon", "natalie.damon@example.com"));
        contacts.Add(new(++key, "Chris Johansson", "chris.johansson@example.com") { Status = false });
    }

    public void Update(Contact updatedContact)
    {
        var old = contacts[updatedContact.Id];
        old = updatedContact;
    }

    public IEnumerable<Contact> Get()
    {
        return contacts;
    }

    public Contact Get(int Id)
    {
        return contacts.Single(c => c.Id == Id);
    }
}