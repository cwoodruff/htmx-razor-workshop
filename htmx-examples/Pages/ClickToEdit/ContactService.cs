namespace htmx_examples.Pages.ClickToEdit;

public class ContactService : IContactService
{
    private Dictionary<int, Contact> contacts;

    public ContactService()
    {
        // Initialize the static contact member.
        contacts = new();
        contacts.Add(1, new("Darth", "Vader", "darth@mustafar.com"));
        ;
    }

    public Contact Get(int Id)
    {
        return contacts[Id];
    }

    public void Update(int Id, Contact updatedContact)
    {
        var old = contacts[Id];
        old = updatedContact;
    }
}