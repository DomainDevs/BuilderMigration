namespace DataToolkit.MigrationBuilder.Models;
public sealed class MigrationArtifact
{
    public int Order { get; set; }
    public string ArtifactKind { get; set; } = "";
    public string Schema { get; set; } = "";
    public string Prefix { get; set; } = "";
    public string Table { get; set; } = "";
    public string DdlFile { get; set; } = "";
    public string? SqlFile { get; set; }
}
