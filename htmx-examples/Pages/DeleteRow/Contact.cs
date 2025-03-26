using System.ComponentModel.DataAnnotations;

namespace htmx_examples.Pages.DeleteRow;

public record Contact(
    int Id,
    [Display(Name = "Name")] string Name,
    [EmailAddress] string Email,
    bool Status = true);