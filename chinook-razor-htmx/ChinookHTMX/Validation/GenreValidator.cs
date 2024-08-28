using ChinookHTMX.Entities;
using FluentValidation;

namespace ChinookHTMX.Validation;

public class GenreValidator : AbstractValidator<Genre>
{
    public GenreValidator()
    {
        RuleFor(g => g.Name).NotNull();
        RuleFor(g => g.Name).MaximumLength(120);
    }
}