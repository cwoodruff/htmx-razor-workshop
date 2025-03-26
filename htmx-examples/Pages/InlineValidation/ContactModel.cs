using System.ComponentModel.DataAnnotations;

namespace htmx_examples.Pages.InlineValidation;

public record Contact(
    [Display(Name = "First Name")] string FirstName,
    [Display(Name = "Last Name")] string LastName,
    [EmailAddress] string Email);