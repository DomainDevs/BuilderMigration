namespace DataToolkit.MigrationBuilder.Models.Medatata2;

public sealed class MetadataOptions
{
    /// <summary>
    /// Esquema a analizar.
    /// Si es null se analizarán todos.
    /// </summary>
    public string? Schema { get; set; } = "dbo";

    /// <summary>
    /// Si se especifica, solamente se analizarán estas tablas.
    /// </summary>
    public ICollection<string>? Tables { get; set; }

    public bool IncludeViews { get; set; }

    public bool IncludeSystemTables { get; set; }

    public bool IncludeExtendedProperties { get; set; } = true;

    public bool IncludeForeignKeys { get; set; } = true;

    public bool IncludeIndexes { get; set; } = true;
}