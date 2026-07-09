using DataToolkit.Library.Connections;
using DataToolkit.Library.Extensions;
using DataToolkit.Provider.SqlServer.Extensions;

namespace DataToolkit.MigrationBuilder.Infrastructure.Connect;

public static class AddDataToolkitConfiguration
{
    public static IServiceCollection AddBuilderDataToolkit(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddDataToolkit(configuration);

        services.AddDataToolkitSqlServer();

        services.AddScoped<DataToolkitContext>();

        return services;
    }
}