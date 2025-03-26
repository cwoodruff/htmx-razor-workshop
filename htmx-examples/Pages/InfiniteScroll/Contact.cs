using System.ComponentModel;

namespace htmx_examples.Pages.InfiniteScroll;

public class Contact
{
    public Contact(string v1, string v2, Guid newGuid)
    {
        this.Name = v1;
        this.Email = v2;
        this.UniqueIdentifier = newGuid.ToString();
    }

    public string? Name { get; set; }
    public string? Email { get; set; }
    [DisplayName("ID")] public string? UniqueIdentifier { get; set; }
}