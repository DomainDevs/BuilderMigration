using DataToolkit.Library.Engine.Abstractions;
using DataToolkit.Library.Engine.Script;
using DataToolkit.MigrationBuilder.Infrastructure.Migration;
using DataToolkit.MigrationBuilder.Services;
using DataToolkit.MigrationBuilder.Services.Migration;
using DataToolkit.MigrationBuilder.Services.Planning;

namespace DataToolkit.MigrationBuilder.Infrastructure.Services;

public static class AddInjectionServices
{
    public static IServiceCollection AddBuilderServices(
        this IServiceCollection services)
    {

        services.AddScoped<MetadataService>();
        services.AddScoped<MigrationMetadataService>();
        services.AddScoped<MigrationWorkFileService>();
        services.AddScoped<MigrationDdlGeneratorService>();
        services.AddScoped<MigrationExtractionGeneratorService>();

        services.AddScoped<MigrationExecutionService>();
        services.AddScoped<ArtifactDiscoveryService>();
        services.AddScoped<DatabaseRuntimeService>();
        
        services.AddScoped<DependencyResolverService>();
        services.AddSingleton<DependencyAnalyzer>();
        services.AddScoped<MigrationPlanningService>();


        services.AddSingleton<ISqlScriptExecutor, SqlScriptExecutor>();

        return services;
    }
}
