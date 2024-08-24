namespace htmx_examples.Pages.DeleteRow
{

    public class ContactService : IContactService
    {
        private List<Contact> contacts;

        public ContactService()
        {
            int key = 0;
            // Initialize the static contact member.
            contacts = new();
            contacts.Add(new(++key, "Joe Smith", "joe@smith.org"));
            contacts.Add(new(++key, "Fuqua Tarkenton", "fuqua@tarkenton.org"));
            contacts.Add(new(++key, "Angie MacDowell", "angie@macdowell.org"));
            contacts.Add(new(++key, "Kim Yee", "kim@yee.org") { Status = false });
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
}