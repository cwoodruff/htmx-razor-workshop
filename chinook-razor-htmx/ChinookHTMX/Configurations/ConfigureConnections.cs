using System.Runtime.InteropServices;
using ChinookHTMX.Data;
using Microsoft.EntityFrameworkCore;

namespace ChinookHTMX.Configurations;

public static class ConfigureConnections
{
    public static IServiceCollection AddConnectionProvider(this IServiceCollection services,
        IConfiguration configuration)
    {
        var connection = configuration.GetConnectionString("Chinook");
        services.AddDbContextPool<ChinookContext>(options => options.UseSqlite(connection));
        return services;
    }
}