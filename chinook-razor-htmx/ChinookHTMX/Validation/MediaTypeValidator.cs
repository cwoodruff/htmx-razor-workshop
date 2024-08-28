using ChinookHTMX.Entities;
using FluentValidation;

namespace ChinookHTMX.Validation;

public class MediaTypeValidator : AbstractValidator<MediaType>
{
    public MediaTypeValidator()
    {
        RuleFor(m => m.Name).NotNull();
        RuleFor(m => m.Name).MaximumLength(120);
    }
}