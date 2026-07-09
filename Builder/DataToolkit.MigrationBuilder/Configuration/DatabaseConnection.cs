namespace DataToolkit.MigrationBuilder.Configuration;

public sealed class DatabaseConnection
{
    public string Provider { get; set; } = string.Empty;

    public string ConnectionString { get; set; } = string.Empty;
}