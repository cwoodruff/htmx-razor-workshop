using ChinookHTMX.Entities;
using FluentValidation;

namespace ChinookHTMX.Validation;

public class AlbumValidator : AbstractValidator<Album>
{
    public AlbumValidator()
    {
        RuleFor(a => a.Title).NotNull();
        RuleFor(a => a.Title).MinimumLength(3);
        RuleFor(a => a.Title).MaximumLength(160);
        RuleFor(a => a.ArtistId).NotNull();
    }
}