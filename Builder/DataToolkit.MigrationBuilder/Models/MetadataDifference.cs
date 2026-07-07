namespace DataToolkit.MigrationBuilder.Models;

public class MetadataDifference
{
    public string Schema { get; set; } = "";
    public string Table { get; set; } = "";
    public string Column { get; set; } = "";
    public string DifferenceType { get; set; } = "";
    public string SourceValue { get; set; } = "";
    public string TargetValue { get; set; } = "";
}
