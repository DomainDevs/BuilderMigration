using DataToolkit.MigrationBuilder.Helpers;

namespace DataToolkit.MigrationBuilder.Models;

public sealed class ReferenceDataMatch
{
    public string SourceValue { get; set; } = string.Empty;

    public string SourceDescription { get; set; } = string.Empty;

    public string TargetValue { get; set; } = string.Empty;

    public string TargetDescription { get; set; } = string.Empty;

    public decimal Confidence { get; set; }

    public MappingStatus Status { get; set; }

    public string? Comment { get; set; }
}