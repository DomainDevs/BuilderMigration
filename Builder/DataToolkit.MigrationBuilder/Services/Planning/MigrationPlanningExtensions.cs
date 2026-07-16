using Microsoft.Extensions.DependencyInjection;

namespace DataToolkit.MigrationBuilder.Services;

public static class MigrationPlanningExtensions
{
    public static IServiceCollection AddMigrationPlanning(
        this IServiceCollection services)
    {
        services.AddScoped<DependencyAnalyzer>();
        services.AddScoped<MigrationPlanningService>();

        return services;
    }
}
