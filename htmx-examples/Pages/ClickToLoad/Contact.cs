using System.ComponentModel;

namespace htmx_examples.Pages.ClickToLoad;

public class Contact
{
    public Contact(string v1, string v2, Guid newGuid)
    {
        this.Name = v1;
        this.Email = v2;
    }

    public string? Name { get; set; }
    public string? Email { get; set; }
}