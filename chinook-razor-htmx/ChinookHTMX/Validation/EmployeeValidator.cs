using ChinookHTMX.Entities;
using FluentValidation;

namespace ChinookHTMX.Validation;

public class EmployeeValidator : AbstractValidator<Employee>
{
    public EmployeeValidator()
    {
        RuleFor(e => e.FirstName).NotNull();
        RuleFor(e => e.LastName).NotNull();
        RuleFor(e => e.Email).EmailAddress();
        RuleFor(e => e.Phone).Matches(@"\(?\d{3}\)?[-\.]? *\d{3}[-\.]? *[-\.]?\d{4}");
        RuleFor(e => e.Fax).Matches(@"\(?\d{3}\)?[-\.]? *\d{3}[-\.]? *[-\.]?\d{4}");
        RuleFor(e => e.FirstName).MaximumLength(20);
        RuleFor(e => e.LastName).MaximumLength(20);
        RuleFor(e => e.Title).MaximumLength(30);
        RuleFor(e => e.Address).MaximumLength(70);
        RuleFor(e => e.City).MaximumLength(40);
        RuleFor(e => e.State).MaximumLength(40);
        RuleFor(e => e.Country).MaximumLength(40);
        RuleFor(e => e.PostalCode).Matches(@"^[0-9]{5}(?:-[0-9]{4})?$");
    }
}