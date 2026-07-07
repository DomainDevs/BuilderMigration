namespace DataToolkit.MigrationBuilder.Models.Medatata2;

/// <summary>
/// Representa la metadata completa de una base de datos.
/// Es el modelo raíz utilizado por todos los componentes del Builder.
/// </summary>
public sealed class DatabaseMetadata
{
    /// <summary>
    /// Servidor.
    /// </summary>
    public string Server { get; set; } = string.Empty;

    /// <summary>
    /// Nombre de la base de datos.
    /// </summary>
    public string Database { get; set; } = string.Empty;

    /// <summary>
    /// Esquema por defecto.
    /// </summary>
    public string DefaultSchema { get; set; } = "dbo";

    /// <summary>
    /// Fecha de extracción de la metadata.
    /// </summary>
    public DateTime ExtractedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Versión de SQL Server.
    /// </summary>
    public string? SqlServerVersion { get; set; }

    /// <summary>
    /// Todas las tablas.
    /// </summary>
    public List<TableMetadata> Tables { get; set; }
        = new();

    /// <summary>
    /// Cantidad de tablas.
    /// </summary>
    public int TableCount
        => Tables.Count;

    /// <summary>
    /// Cantidad total de columnas.
    /// </summary>
    public int ColumnCount
        => Tables.Sum(t => t.ColumnCount);

    /// <summary>
    /// Tablas con PK.
    /// </summary>
    public IEnumerable<TableMetadata> PrimaryKeyTables
        => Tables.Where(t => t.HasPrimaryKey);

    /// <summary>
    /// Tablas con FK.
    /// </summary>
    public IEnumerable<TableMetadata> ForeignKeyTables
        => Tables.Where(t => t.HasForeignKeys);

    /// <summary>
    /// Tablas con Identity.
    /// </summary>
    public IEnumerable<TableMetadata> IdentityTables
        => Tables.Where(t => t.HasIdentity);

    /// <summary>
    /// Tablas paramétricas.
    /// </summary>
    public IEnumerable<TableMetadata> ReferenceTables
        => Tables.Where(t => t.IsReferenceTable);

    /// <summary>
    /// Tablas que requieren homologación.
    /// </summary>
    public IEnumerable<TableMetadata> HomologationTables
        => Tables.Where(t => t.RequiresHomologation);

    /// <summary>
    /// Orden sugerido de migración.
    /// </summary>
    public IEnumerable<TableMetadata> MigrationOrder
        => Tables
            .OrderBy(t => t.DependencyLevel)
            .ThenBy(t => t.Name);

    /// <summary>
    /// Busca una tabla.
    /// </summary>
    public TableMetadata? FindTable(
        string tableName)
    {
        return Tables.FirstOrDefault(t =>
            t.Name.Equals(
                tableName,
                StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Busca una tabla por esquema.
    /// </summary>
    public TableMetadata? FindTable(
        string schema,
        string tableName)
    {
        return Tables.FirstOrDefault(t =>
            t.Schema.Equals(
                schema,
                StringComparison.OrdinalIgnoreCase)
            &&
            t.Name.Equals(
                tableName,
                StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Indica si existe una tabla.
    /// </summary>
    public bool ContainsTable(
        string tableName)
    {
        return FindTable(tableName) != null;
    }

    /// <summary>
    /// Obtiene todas las columnas.
    /// </summary>
    public IEnumerable<ColumnMetadata> GetColumns()
    {
        return Tables.SelectMany(t => t.Columns);
    }

    /// <summary>
    /// Obtiene todas las PK.
    /// </summary>
    public IEnumerable<ColumnMetadata> GetPrimaryKeys()
    {
        return Tables
            .SelectMany(t => t.PrimaryKeys);
    }

    /// <summary>
    /// Obtiene todas las FK.
    /// </summary>
    public IEnumerable<ColumnMetadata> GetForeignKeys()
    {
        return Tables
            .SelectMany(t => t.ForeignKeys);
    }

    /// <summary>
    /// Obtiene todas las columnas Identity.
    /// </summary>
    public IEnumerable<ColumnMetadata> GetIdentityColumns()
    {
        return Tables
            .SelectMany(t => t.IdentityColumns);
    }

    /// <summary>
    /// Limpia tablas vacías.
    /// </summary>
    public void RemoveEmptyTables()
    {
        Tables.RemoveAll(t => t.ColumnCount == 0);
    }

    /// <summary>
    /// Ordena las tablas por nombre.
    /// </summary>
    public void Sort()
    {
        Tables = Tables
            .OrderBy(t => t.Schema)
            .ThenBy(t => t.Name)
            .ToList();
    }

    public override string ToString()
    {
        return $"{Server} / {Database}";
    }
}