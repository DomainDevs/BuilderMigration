namespace DataToolkit.MigrationBuilder.Infrastructure;

public sealed class DatabaseConnectionOptions
{
    public string Provider { get; set; } = string.Empty;

    public string Servidor { get; set; } = string.Empty;

    public string BaseDatos { get; set; } = string.Empty;

    public string Usuario { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public bool PersistSecurityInfo { get; set; }
}