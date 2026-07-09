using DataToolkit.Library.Common;

namespace DataToolkit.MigrationBuilder.Configuration;

public sealed class DataToolkitOptions
{
    public const string SectionName = "DataToolkit";

    public DatabaseConnection Source { get; set; } = new();

    public DatabaseConnection Target { get; set; } = new();

    public LoggingOptions Logging { get; set; } = new();

    public TelemetryOptions Telemetry { get; set; } = new();

    public RetryOptions Retry { get; set; } = new();
}