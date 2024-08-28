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
            contacts.Add(++key, new(key, "Bobby Jones", "bobby@jones.org"));
            contacts.Add(++key, new(key, "Sally Ride", "sally@apace.org"));
            contacts.Add(++key, new(key, "Brian Woodruff", "dr.brian@doctor.org"));
            contacts.Add(++key, new(key, "Spencer Woodruff", "spencer@woodruff.org") { Status = false });
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