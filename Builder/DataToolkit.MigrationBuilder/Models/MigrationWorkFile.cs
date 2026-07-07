namespace DataToolkit.MigrationBuilder.Models;

public sealed class MigrationWorkFile
{
    public string Schema { get; set; } = "";

    public string Table { get; set; } = "";

    public string SourceTable { get; set; } = "";

    public string TargetTable { get; set; } = "";

    public List<ColumnMapping> Columns { get; set; } = [];

    public string? DefaultValue { get; set; }

    public string? Notes { get; set; }
}
