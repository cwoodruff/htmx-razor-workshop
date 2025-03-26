using System.ComponentModel.DataAnnotations;

namespace htmx_examples.Pages.BulkUpdate;

public record Contact(int Id, [Display(Name = "Name")] string Name, [EmailAddress] string Email)
{
    public bool Status { get; set; } = true;
    public bool Updated { get; set; } = false;
}