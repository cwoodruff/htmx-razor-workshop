using ChinookHTMX.Entities;
using ChinookHTMX.Validation;
using FluentValidation;

namespace ChinookHTMX.Configurations;

public static class ServicesConfiguration
{
    public static void ConfigureValidators(this IServiceCollection services)
    {
        services.AddScoped<IValidator<Album>, AlbumValidator>();
        services.AddScoped<IValidator<Artist>, ArtistValidator>();
        services.AddScoped<IValidator<Customer>, CustomerValidator>();
        services.AddScoped<IValidator<Employee>, EmployeeValidator>();
        services.AddScoped<IValidator<Genre>, GenreValidator>();
        services.AddScoped<IValidator<Invoice>, InvoiceValidator>();
        services.AddScoped<IValidator<InvoiceLine>, InvoiceLineValidator>();
        services.AddScoped<IValidator<MediaType>, MediaTypeValidator>();
        services.AddScoped<IValidator<Playlist>, PlaylistValidator>();
        services.AddScoped<IValidator<Track>, TrackValidator>();
    }
}