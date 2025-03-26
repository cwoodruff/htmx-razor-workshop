namespace htmx_examples.Pages.BulkUpdate;

public class ContactService : IContactService
{
    private Dictionary<int, Contact> contacts;

    public ContactService()
    {
        int key = 0;
        // Initialize the static contact member.
        contacts = new();
        contacts.Add(++key, new(key, "Bobby Jones", "bobby@jones.org"));
        contacts.Add(++key, new(key, "Sally Ride", "sally@apace.org"));
        contacts.Add(++key, new(key, "Brian Woodruff", "dr.brian@doctor.org"));
        contacts.Add(++key, new(key, "Spencer Woodruff", "spencer@woodruff.org") { Status = false });
    }

    public IEnumerable<Contact> Get()
    {
        return contacts.Select(c => c.Value);
    }

    public void Update(int Id, bool status)
    {
        contacts[Id].Status = status;
        contacts[Id].Updated = false;
    }
}