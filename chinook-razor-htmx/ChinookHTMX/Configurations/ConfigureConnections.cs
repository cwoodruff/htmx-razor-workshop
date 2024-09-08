using System.Runtime.InteropServices;
using ChinookHTMX.Data;
using Microsoft.EntityFrameworkCore;

namespace ChinookHTMX.Configurations;

public static class ConfigureConnections
{
    public static IServiceCollection AddConnectionProvider(this IServiceCollection services,
        IConfiguration configuration)
    {
        var connection = String.Empty;

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            connection = configuration.GetConnectionString("ChinookDbWindows");
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) || RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            connection = configuration.GetConnectionString("ChinookDbDocker");

        services.AddDbContextPool<ChinookContext>(options => options.UseSqlServer(connection));

        return services;
    }
}