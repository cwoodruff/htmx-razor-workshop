using ChinookHTMX.Entities;
using FluentValidation;

namespace ChinookHTMX.Validation;

public class PlaylistValidator : AbstractValidator<Playlist>
{
    public PlaylistValidator()
    {
        RuleFor(p => p.Name).NotNull();
        RuleFor(p => p.Name).MaximumLength(120);
    }
}