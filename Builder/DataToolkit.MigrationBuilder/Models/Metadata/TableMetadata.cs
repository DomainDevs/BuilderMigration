namespace DataToolkit.MigrationBuilder.Models.Metadata;

public class TableMetadata
{
    public string Schema { get; set; } = "";
    public string Name { get; set; } = "";
    public List<ColumnMetadata> Columns { get; set; } = new();
}
