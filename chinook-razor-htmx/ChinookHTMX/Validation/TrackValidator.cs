using ChinookHTMX.Entities;
using FluentValidation;

namespace ChinookHTMX.Validation;

public class TrackValidator : AbstractValidator<Track>
{
    public TrackValidator()
    {
        RuleFor(t => t.Name).NotNull();
        RuleFor(t => t.Name).MaximumLength(200);
        RuleFor(t => t.Bytes).GreaterThan(0);
        RuleFor(t => t.Milliseconds).GreaterThan(0);
        RuleFor(t => t.Composer).NotNull();
        RuleFor(t => t.Composer).MaximumLength(220);
        RuleFor(t => t.UnitPrice).GreaterThan(0);
        RuleFor(t => t.UnitPrice).LessThanOrEqualTo((decimal)9.99);
        RuleFor(t => t.AlbumId).NotNull();
        RuleFor(t => t.GenreId).NotNull();
        RuleFor(t => t.MediaTypeId).NotNull();
    }
}