namespace DataToolkit.MigrationBuilder.Models;

public sealed class MigrationTableResult
{
    public string Table { get; set; } = string.Empty;

    public long StagingRows { get; set; }

    public long DestinationRows { get; set; }

    public bool Success { get; set; }

    public string Message { get; set; } = string.Empty;
}