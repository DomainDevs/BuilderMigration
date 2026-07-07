using DataToolkit.MigrationBuilder.Configuration;
using DataToolkit.MigrationBuilder.Models.Metadata;
using Microsoft.Extensions.Options;

namespace DataToolkit.MigrationBuilder.Services.Migration;


public class MigrationMetadataService
{

    private readonly MigrationOptions _options;

    public MigrationMetadataService(
        IOptions<MigrationOptions> options)
    {
        _options = options.Value;
    }

    public string pathconfigure(int optselect = 0)
    {
        if (optselect == 0) {
            return _options.WorkFilePath;
        }
        else
        {
            return _options.MetadataOutPut;
        }
            
    }
    
    /// <summary>
    /// Compara metadata y devuelve solo las diferencias.
    /// </summary>
    public List<MetadataDifference> CompareMetadata(
        List<TableMetadata> sourceMetadata,
        List<TableMetadata> targetMetadata)
    {
        var differences = new List<MetadataDifference>();

        foreach (var sourceTable in sourceMetadata)
        {
            var targetTable = targetMetadata
                .FirstOrDefault(t => t.Schema == sourceTable.Schema && t.Name == sourceTable.Name);

            if (targetTable == null)
            {
                differences.Add(new MetadataDifference
                {
                    Schema = sourceTable.Schema,
                    Table = sourceTable.Name,
                    DifferenceType = "Tabla faltante en destino"
                });
                continue;
            }

            foreach (var sourceColumn in sourceTable.Columns)
            {
                var targetColumn = targetTable.Columns
                    .FirstOrDefault(c => c.Name == sourceColumn.Name);

                if (targetColumn == null)
                {
                    differences.Add(new MetadataDifference
                    {
                        Schema = sourceTable.Schema,
                        Table = sourceTable.Name,
                        Column = sourceColumn.Name,
                        DifferenceType = "Columna faltante en destino"
                    });
                    continue;
                }

                //if (sourceColumn.SqlType != targetColumn.SqlType)
                if (!Eq(sourceColumn.SqlType, targetColumn.SqlType))
                    differences.Add(CreateDifference(sourceTable, sourceColumn,
                        "Tipo de dato distinto", sourceColumn.SqlType, targetColumn.SqlType));

                //if (sourceColumn.MaxLength != targetColumn.MaxLength)
                if (!Eq(sourceColumn.MaxLength, targetColumn.MaxLength))
                    differences.Add(CreateDifference(sourceTable, sourceColumn,
                        "MaxLength distinto", sourceColumn.MaxLength, targetColumn.MaxLength));

                if (sourceColumn.IsNullable != targetColumn.IsNullable)
                    differences.Add(CreateDifference(sourceTable, sourceColumn,
                        "Nullable distinto", sourceColumn.IsNullable.ToString(), targetColumn.IsNullable.ToString()));

                if (sourceColumn.IsIdentity != targetColumn.IsIdentity)
                    differences.Add(CreateDifference(sourceTable, sourceColumn,
                        "Identity distinto", sourceColumn.IsIdentity.ToString(), targetColumn.IsIdentity.ToString()));

                if (sourceColumn.IsPrimaryKey != targetColumn.IsPrimaryKey)
                    differences.Add(CreateDifference(sourceTable, sourceColumn,
                        "PK distinto", sourceColumn.IsPrimaryKey.ToString(), targetColumn.IsPrimaryKey.ToString()));

                //if (sourceColumn.ForeignTable != targetColumn.ForeignTable ||
                //sourceColumn.ForeignColumn != targetColumn.ForeignColumn)
                if (!Eq(sourceColumn.ForeignTable, targetColumn.ForeignTable) ||
                        !Eq(sourceColumn.ForeignColumn, targetColumn.ForeignColumn))

                    differences.Add(CreateDifference(sourceTable, sourceColumn,
                    "FK distinto",
                    string.Format("{0}.{1}",
                        sourceColumn.ForeignTable ?? "",
                        sourceColumn.ForeignColumn ?? ""),
                    string.Format("{0}.{1}",
                        targetColumn.ForeignTable ?? "",
                        targetColumn.ForeignColumn ?? "")
                    ));
            }
        }

        foreach (var targetTable in targetMetadata)
        {
            var sourceTable = sourceMetadata
                //.FirstOrDefault(t => t.Schema == targetTable.Schema && t.Name == targetTable.Name);
                .FirstOrDefault(t =>
                string.Equals(t.Schema, targetTable.Schema, StringComparison.OrdinalIgnoreCase)
                && string.Equals(t.Name, targetTable.Name, StringComparison.OrdinalIgnoreCase));

            if (sourceTable == null)
            {
                differences.Add(new MetadataDifference
                {
                    Schema = targetTable.Schema,
                    Table = targetTable.Name,
                    DifferenceType = "Tabla extra en destino"
                });
            }
        }

        return differences;
    }

    private static bool Eq(string? a, string? b)
    {
        return string.Equals(a?.Trim(), b?.Trim(), StringComparison.OrdinalIgnoreCase);
    }

    private MetadataDifference CreateDifference(
        TableMetadata table,
        ColumnMetadata column,
        string type,
        string sourceVal,
        string targetVal)
    {
        return new MetadataDifference
        {
            Schema = table.Schema,
            Table = table.Name,
            Column = column.Name,
            DifferenceType = type,
            SourceValue = sourceVal,
            TargetValue = targetVal
        };
    }

    public async Task GenerateWorkFilesAsync(
        List<TableMetadata> sourceMetadata,
        List<TableMetadata> targetMetadata,
        string outputPath)
    {
        
        if (!Directory.Exists(outputPath))
            Directory.CreateDirectory(outputPath);

        foreach (var sourceTable in sourceMetadata)
        {
            var targetTable = targetMetadata
                .FirstOrDefault(t =>
                    string.Equals(t.Schema, sourceTable.Schema, StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(t.Name, sourceTable.Name, StringComparison.OrdinalIgnoreCase));

            var sb = new StringBuilder();

            sb.AppendLine("Campo\tNombre\tTipo/Long\tRequerido\tFormato\tDescripción\tObservación\tSolicitud de SISTRAN\tDatos del Cliente\tTablas Relacionadas\tValidaciones\tComentarios / Status\tTipo Conversion\tProceso de Conversion o Forzamiento");
            sb.AppendLine();

            var usedTargetColumns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            int i = 1;

            foreach (var srcCol in sourceTable.Columns)
            {
                var tgtCol = targetTable?.Columns
                    .FirstOrDefault(c => c.Name == srcCol.Name);

                usedTargetColumns.Add(srcCol.Name);

                var row = BuildRow(i++, srcCol, tgtCol, sourceTable);
                sb.AppendLine(row);
            }

            // ➕ columnas nuevas en destino
            if (targetTable != null)
            {
                foreach (var tgtCol in targetTable.Columns)
                {
                    if (usedTargetColumns.Contains(tgtCol.Name))
                        continue;

                    sb.AppendLine(BuildRow(i++, null, tgtCol, sourceTable, isOrphan: true));
                }

                // 🔑 PK al final
                var pkCols = targetTable.Columns.Where(c => c.IsPrimaryKey).ToList();
                if (pkCols.Any())
                {
                    sb.AppendLine();
                    sb.AppendLine($"CLAVES:\tprimary key ( {string.Join(",", pkCols.Select(x => x.Name))} )");
                }
            }
            
            var file = Path.Combine(outputPath, $"{sourceTable.Schema}_{sourceTable.Name}_WFR.txt");
            await File.WriteAllTextAsync(file, sb.ToString(), Encoding.UTF8);
        }
    }

    private string BuildRow(
        int index,
        ColumnMetadata? source,
        ColumnMetadata? target,
        TableMetadata table,
        bool isOrphan = false)
    {
        string campo = source?.Name ?? target?.Name ?? "";

        string tipo = BuildType(source, target);
        string requerido = BuildRequired(source, target);

        string descripcion = "";
        string observacion = "";

        string conversion = "";
        string proceso = "";

        if (source == null && target != null)
        {
            // columna nueva en destino
            descripcion = "Columna existe solo en destino (HUÉRFANA del origen)";
            conversion = "REVISAR USO";
            proceso = "POSIBLE ELIMINACIÓN O AJUSTE";
        }
        else if (source != null && target == null)
        {
            // nueva columna origen
            descripcion = "Columna nueva en origen (DEBE MIGRARSE)";
            conversion = "AGREGAR EN DESTINO";
            proceso = "ALTER TABLE ADD COLUMN";
        }
        else if (source != null && target != null)
        {
            if (!Eq(source.SqlType, target.SqlType))
                observacion += "Tipo diferente; ";

            if (!Eq(source.MaxLength, target.MaxLength))
            {
                observacion += "Longitud diferente; ";

                if (int.TryParse(source.MaxLength, out var s) &&
                    int.TryParse(target.MaxLength, out var t) &&
                    s > t)
                {
                    conversion = "EXPANDIR";
                    proceso = "ALTER COLUMN INCREASE SIZE";
                }
            }

            if (source.IsNullable != target.IsNullable)
                observacion += "Nullability diferente; ";

            if (string.IsNullOrEmpty(conversion))
            {
                conversion = "SIN CAMBIO";
                proceso = "OK";
            }
        }

        return string.Join("\t", new[]
        {
        index.ToString(),
        campo,
        tipo,
        requerido,
        "", // Formato
        "", // Descripción base (ya incluida arriba)
        observacion,
        "", "", "", "", "",
        conversion,
        proceso
    });
    }

    //helpers simples
    private string BuildType(ColumnMetadata? source, ColumnMetadata? target)
    {
        var col = source ?? target;
        if (col == null) return "";

        return $"{col.SqlType}({col.MaxLength ?? ""})";
    }

    private string BuildRequired(ColumnMetadata? source, ColumnMetadata? target)
    {
        var col = source ?? target;
        if (col == null) return "";

        if (col.IsNullable) return "No";
        return "Si";
    }

}