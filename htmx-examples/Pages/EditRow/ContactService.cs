namespace htmx_examples.Pages.EditRow
{

    public class ContactService : IContactService
    {
        private List<Contact> contacts;

        public ContactService()
        {
            int key = 0;
            // Initialize the static contact member.
            contacts = new();
            contacts.Add(new(++key, "Joe Smith  ", "joe @smith.org"));
            contacts.Add(new(++key, "Fuqua Tarkenton", "fuqua @tarkenton.org"));
            contacts.Add(new(++key, "Angie MacDowell", "angie @macdowell.org"));
            contacts.Add(new(++key, "Kim Yee", "kim @yee.org") { Status = false });
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
            return contacts.Single(c =>c.Id == Id);
        }
    }
}