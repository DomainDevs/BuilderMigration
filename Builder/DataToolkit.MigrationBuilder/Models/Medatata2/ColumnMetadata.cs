namespace DataToolkit.MigrationBuilder.Models.Medatata2;

/// <summary>
/// Metadata completa de una columna de base de datos.
/// Es el modelo base utilizado por el Builder para:
/// - Comparación de esquemas
/// - Generación de DDL
/// - WorkFiles
/// - Homologación
/// </summary>
public sealed class ColumnMetadata
{
    /// <summary>
    /// Nombre de la columna.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Tipo SQL original.
    /// </summary>
    public string SqlType { get; set; } = string.Empty;

    /// <summary>
    /// Longitud máxima.
    /// -1 representa MAX.
    /// </summary>
    public int? MaxLength { get; set; }

    /// <summary>
    /// Precisión.
    /// </summary>
    public int? Precision { get; set; }

    /// <summary>
    /// Escala.
    /// </summary>
    public int? Scale { get; set; }

    /// <summary>
    /// Posición dentro de la tabla.
    /// </summary>
    public int Ordinal { get; set; }

    /// <summary>
    /// Permite NULL.
    /// </summary>
    public bool IsNullable { get; set; }

    /// <summary>
    /// Es Identity.
    /// </summary>
    public bool IsIdentity { get; set; }

    /// <summary>
    /// Es columna calculada.
    /// </summary>
    public bool IsComputed { get; set; }

    /// <summary>
    /// Definición de columna calculada.
    /// </summary>
    public string? ComputedDefinition { get; set; }

    /// <summary>
    /// Valor por defecto.
    /// </summary>
    public string? DefaultValue { get; set; }

    /// <summary>
    /// Collation.
    /// </summary>
    public string? Collation { get; set; }

    /// <summary>
    /// Es Primary Key.
    /// </summary>
    public bool IsPrimaryKey { get; set; }

    public string? PrimaryKeyName { get; set; }

    /// <summary>
    /// Es Foreign Key.
    /// </summary>
    public bool IsForeignKey { get; set; }

    public string? ForeignKeyName { get; set; }

    public string? ForeignTable { get; set; }

    public string? ForeignColumn { get; set; }

    public string? FK_DeleteAction { get; set; }

    public string? FK_UpdateAction { get; set; }

    public bool FK_IsDisabled { get; set; }

    public bool FK_IsNotTrusted { get; set; }

    /// <summary>
    /// Tiene índice.
    /// </summary>
    public bool HasIndex { get; set; }

    /// <summary>
    /// Índice único.
    /// </summary>
    public bool IsUniqueIndex { get; set; }

    public string? IndexName { get; set; }

    /// <summary>
    /// Columna RowGuid.
    /// </summary>
    public bool IsRowGuid { get; set; }

    /// <summary>
    /// Tipo CLR equivalente.
    /// </summary>
    public string? ClrType { get; set; }

    /// <summary>
    /// Tipo SQL normalizado por SqlTypeMapper.
    /// </summary>
    public string? NormalizedSqlType { get; set; }

    /// <summary>
    /// Descripción (MS_Description).
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Indica si existe únicamente en el origen.
    /// </summary>
    public bool ExistsOnlyInSource { get; set; }

    /// <summary>
    /// Indica si existe únicamente en el destino.
    /// </summary>
    public bool ExistsOnlyInTarget { get; set; }

    /// <summary>
    /// Indica si la columna fue homologada.
    /// </summary>
    public bool IsMapped { get; set; }

    /// <summary>
    /// Puntaje de similitud utilizado por Compare y WorkFiles.
    /// </summary>
    public decimal MatchScore { get; set; }

    /// <summary>
    /// Columna equivalente encontrada.
    /// </summary>
    public ColumnMetadata? MatchedColumn { get; set; }

    public override string ToString()
    {
        return $"{Name} ({SqlType})";
    }
}