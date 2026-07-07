namespace DataToolkit.MigrationBuilder.Models.Medatata2;

/// <summary>
/// Representa la metadata completa de una tabla.
/// Es el modelo principal utilizado por el Builder para:
/// - Comparación de esquemas
/// - Generación de DDL
/// - WorkFiles
/// - Homologación
/// - Dependencias
/// </summary>
public sealed class TableMetadata
{
    /// <summary>
    /// Esquema.
    /// </summary>
    public string Schema { get; set; } = "dbo";

    /// <summary>
    /// Nombre de la tabla.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Nombre completo.
    /// </summary>
    public string FullName
        => $"[{Schema}].[{Name}]";

    /// <summary>
    /// Columnas.
    /// </summary>
    public List<ColumnMetadata> Columns { get; set; }
        = new();

    /// <summary>
    /// Llaves primarias.
    /// </summary>
    public IReadOnlyList<ColumnMetadata> PrimaryKeys
        => Columns
            .Where(c => c.IsPrimaryKey)
            .ToList();

    /// <summary>
    /// Llaves foráneas.
    /// </summary>
    public IReadOnlyList<ColumnMetadata> ForeignKeys
        => Columns
            .Where(c => c.IsForeignKey)
            .ToList();

    /// <summary>
    /// Columnas Identity.
    /// </summary>
    public IReadOnlyList<ColumnMetadata> IdentityColumns
        => Columns
            .Where(c => c.IsIdentity)
            .ToList();

    /// <summary>
    /// Columnas calculadas.
    /// </summary>
    public IReadOnlyList<ColumnMetadata> ComputedColumns
        => Columns
            .Where(c => c.IsComputed)
            .ToList();

    /// <summary>
    /// Columnas con índices.
    /// </summary>
    public IReadOnlyList<ColumnMetadata> IndexedColumns
        => Columns
            .Where(c => c.HasIndex)
            .ToList();

    /// <summary>
    /// Cantidad de columnas.
    /// </summary>
    public int ColumnCount
        => Columns.Count;

    /// <summary>
    /// Cantidad de PK.
    /// </summary>
    public int PrimaryKeyCount
        => PrimaryKeys.Count;

    /// <summary>
    /// Cantidad de FK.
    /// </summary>
    public int ForeignKeyCount
        => ForeignKeys.Count;

    /// <summary>
    /// Cantidad de columnas Identity.
    /// </summary>
    public int IdentityCount
        => IdentityColumns.Count;

    /// <summary>
    /// Indica si tiene Identity.
    /// </summary>
    public bool HasIdentity
        => IdentityColumns.Count > 0;

    /// <summary>
    /// Indica si tiene FK.
    /// </summary>
    public bool HasForeignKeys
        => ForeignKeyCount > 0;

    /// <summary>
    /// Indica si tiene PK.
    /// </summary>
    public bool HasPrimaryKey
        => PrimaryKeyCount > 0;

    /// <summary>
    /// Tabla solamente existe en origen.
    /// </summary>
    public bool ExistsOnlyInSource { get; set; }

    /// <summary>
    /// Tabla solamente existe en destino.
    /// </summary>
    public bool ExistsOnlyInTarget { get; set; }

    /// <summary>
    /// Tabla paramétrica.
    /// </summary>
    public bool IsReferenceTable { get; set; }

    /// <summary>
    /// Tabla de homologación.
    /// </summary>
    public bool RequiresHomologation { get; set; }

    /// <summary>
    /// Nivel de dependencia.
    /// </summary>
    public int DependencyLevel { get; set; }

    /// <summary>
    /// Orden sugerido de migración.
    /// </summary>
    public int MigrationOrder { get; set; }

    /// <summary>
    /// Descripción (MS_Description).
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Busca una columna por nombre.
    /// </summary>
    public ColumnMetadata? FindColumn(string name)
    {
        return Columns.FirstOrDefault(c =>
            c.Name.Equals(
                name,
                StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Indica si existe una columna.
    /// </summary>
    public bool ContainsColumn(string name)
    {
        return FindColumn(name) != null;
    }

    /// <summary>
    /// Devuelve las columnas no calculadas.
    /// </summary>
    public IEnumerable<ColumnMetadata> GetInsertableColumns()
    {
        return Columns.Where(c => !c.IsComputed);
    }

    public override string ToString()
    {
        return FullName;
    }
}
