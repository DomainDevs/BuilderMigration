namespace DataToolkit.MigrationBuilder.Models;

public sealed class TableDependency
{
    public string Table { get; set; } = string.Empty;

    public string Column { get; set; } = string.Empty;

    public string ForeignTable { get; set; } = string.Empty;

    public string ForeignColumn { get; set; } = string.Empty;

    public int Level { get; set; }
}