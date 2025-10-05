using System.Reflection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace htmx_examples_blazor;

public static class EndpointExtensions
{
    public static IServiceCollection AddEndpoints(this IServiceCollection services, Assembly assembly)
    {
        ServiceDescriptor[] serviceDescriptors = assembly.DefinedTypes
            .Where(type => type is { IsAbstract: false, IsInterface: false } && type.IsAssignableTo(typeof(IEndpoints)))
            .Select(type => ServiceDescriptor.Transient(typeof(IEndpoints), type))
            .ToArray();

        services.TryAddEnumerable(serviceDescriptors);

        return services;
    }

    public static IApplicationBuilder MapEndpoints(this WebApplication app, RouteGroupBuilder? routeGroupBuilder = null)
    {
        IEnumerable<IEndpoints> endpoints = app.Services.GetRequiredService<IEnumerable<IEndpoints>>();
        IEndpointRouteBuilder builder = routeGroupBuilder is null ? app : routeGroupBuilder;
        foreach (IEndpoints endpoint in endpoints)
        {
            endpoint.MapEndpoints(builder);
        }

        return app;
    }
}
