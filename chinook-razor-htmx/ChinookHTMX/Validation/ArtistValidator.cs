using ChinookHTMX.Entities;
using FluentValidation;

namespace ChinookHTMX.Validation;

public class ArtistValidator : AbstractValidator<Artist>
{
    public ArtistValidator()
    {
        RuleFor(a => a.Name).NotNull();
        RuleFor(a => a.Name).MaximumLength(120);
    }
}