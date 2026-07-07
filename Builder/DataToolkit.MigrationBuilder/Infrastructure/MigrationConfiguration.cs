using DataToolkit.MigrationBuilder.Configuration;

namespace DataToolkit.MigrationBuilder.Infrastructure;

public sealed class MigrationConfiguration
{
    public DatabaseConnectionOptions SourceDB { get; set; } = new();

    public DatabaseConnectionOptions DestinationDB { get; set; } = new();

    public MigrationOptions Migration { get; set; } = new();
}