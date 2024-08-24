namespace htmx_examples.Pages.BulkUpdate
{

    public class ContactService : IContactService
    {
        private Dictionary<int, Contact> contacts;

        public ContactService()
        {
            int key = 0;
            // Initialize the static contact member.
            contacts = new();
            contacts.Add(++key, new(key, "Joe Smith  ", "joe @smith.org"));
            contacts.Add(++key, new(key, "Angie MacDowell", "angie @macdowell.org"));
            contacts.Add(++key, new(key, "Fuqua Tarkenton", "fuqua @tarkenton.org"));
            contacts.Add(++key, new(key, "Kim Yee", "kim @yee.org") { Status = false });
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
}