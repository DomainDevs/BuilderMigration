using DataToolkit.Library.Connections;
using DataToolkit.Library.Extensions;
using DataToolkit.MigrationBuilder.Configuration;
using DataToolkit.MigrationBuilder.Services;
using DataToolkit.MigrationBuilder.Services.Migration;
using DataToolkit.Provider.SqlServer.Connections.Providers;
using DataToolkit.Provider.SqlServer.Extensions;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Data.Common;

namespace DataToolkit.MigrationBuilder.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddBuilderServices(
        this IServiceCollection services)
    {

        services.AddScoped<MetadataService>();
        services.AddScoped<MigrationMetadataService>();
        services.AddScoped<MigrationWorkFileService>();
        services.AddScoped<MigrationDdlGeneratorService>();
        services.AddScoped<MigrationExtractionGeneratorService>();

        return services;
    }
}
