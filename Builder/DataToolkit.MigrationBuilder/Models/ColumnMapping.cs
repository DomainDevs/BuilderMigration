namespace DataToolkit.MigrationBuilder.Models;

public sealed class ColumnMapping
{
    public string? SourceColumn { get; set; } = "";

    public string? TargetColumn { get; set; } = "";

    public string Rule { get; set; } = "DIRECT";

    public bool Required { get; set; }

    public string? DefaultValue { get; set; }

    public string? Notes { get; set; }
}
