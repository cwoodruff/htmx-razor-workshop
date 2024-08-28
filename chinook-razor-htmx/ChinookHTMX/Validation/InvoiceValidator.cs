using ChinookHTMX.Entities;
using FluentValidation;

namespace ChinookHTMX.Validation;

public class InvoiceValidator : AbstractValidator<Invoice>
{
    public InvoiceValidator()
    {
        RuleFor(i => i.CustomerId).NotNull();
        RuleFor(i => i.InvoiceDate).NotNull();
        RuleFor(i => i.Total).NotNull();
        RuleFor(i => i.Total).GreaterThan(0);
        RuleFor(i => i.BillingAddress).NotNull();
        RuleFor(i => i.BillingCity).NotNull();
        RuleFor(i => i.BillingCountry).NotNull();
        RuleFor(i => i.BillingState).NotNull();
        RuleFor(i => i.BillingPostalCode).NotNull();
        RuleFor(i => i.BillingAddress).MaximumLength(70);
        RuleFor(i => i.BillingCity).MaximumLength(40);
        RuleFor(i => i.BillingCountry).MaximumLength(40);
        RuleFor(i => i.BillingState).MaximumLength(40);
        RuleFor(i => i.BillingPostalCode).Matches(@"^[0-9]{5}(?:-[0-9]{4})?$");
    }
}