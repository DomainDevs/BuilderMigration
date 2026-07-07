using DataToolkit.MigrationBuilder.Models.Connections;
using DataToolkit.MigrationBuilder.Models.Metadata;

namespace DataToolkit.MigrationBuilder.Services;

public class MetadataService
{
    public async Task<List<TableMetadata>> ExtractMetadataAsync(
        ConnectionRequest connection,
        string? schema = null,
        List<string>? tables = null)
    {
        var metadata = new List<TableMetadata>();

        var cs = new SqlConnectionStringBuilder
        {
            DataSource = connection.Server,
            InitialCatalog = connection.Database,
            UserID = connection.User,
            Password = connection.Password,
            TrustServerCertificate = true,
            Encrypt = false
        };

        await using var conn =
            new SqlConnection(cs.ConnectionString);

        await conn.OpenAsync();

        var filters = new List<string>();

        if (!string.IsNullOrWhiteSpace(schema))
        {
            filters.Add("s.name = @Schema");
        }

        if (tables is { Count: > 0 })
        {
            var tableFilter = string.Join(",",
                tables.Select(t =>
                    $"'{t.Replace("'", "''")}'"));

            filters.Add($"t.name IN ({tableFilter})");
        }

        var whereClause =
            filters.Count > 0
                ? $"WHERE {string.Join(" AND ", filters)}"
                : string.Empty;

        using var cmd = conn.CreateCommand();

        cmd.CommandText = $@"
SELECT
    s.name AS SchemaName,
    t.name AS TableName,
    c.name AS ColumnName,
    ty.name AS DataType,

    CASE
        WHEN c.max_length = -1 THEN 'MAX'
        WHEN ty.name IN ('nvarchar','nchar')
            THEN CAST(c.max_length / 2 AS VARCHAR(10))
        ELSE CAST(c.max_length AS VARCHAR(10))
    END AS MaxLength,

    c.precision AS Precision,
    c.scale AS Scale,

    CASE
        WHEN c.is_nullable = 1 THEN 'YES'
        ELSE 'NO'
    END AS IsNullable,

    CASE
        WHEN c.is_identity = 1 THEN 'YES'
        ELSE 'NO'
    END AS IsIdentity,

    CASE
        WHEN c.is_computed = 1 THEN 'YES'
        ELSE 'NO'
    END AS IsComputed,

    c.collation_name AS Collation,

    dc.definition AS DefaultValue,

    CASE
        WHEN pk.column_id IS NOT NULL THEN 'YES'
        ELSE 'NO'
    END AS IsPrimaryKey,

    pk.constraint_name AS PrimaryKeyName,

    rt.name AS ForeignTable,

    rc.name AS ForeignColumn,

    fkref.name AS ForeignKeyName,

    fkref.delete_referential_action_desc
        AS FK_DeleteAction,

    fkref.update_referential_action_desc
        AS FK_UpdateAction,

    fkref.is_disabled
        AS FK_IsDisabled,

    fkref.is_not_trusted
        AS FK_IsNotTrusted

FROM sys.schemas s

INNER JOIN sys.tables t
    ON t.schema_id = s.schema_id

INNER JOIN sys.columns c
    ON c.object_id = t.object_id

INNER JOIN sys.types ty
    ON c.user_type_id = ty.user_type_id

LEFT JOIN sys.default_constraints dc
    ON c.default_object_id = dc.object_id

LEFT JOIN
(
    SELECT
        ic.object_id,
        ic.column_id,
        i.name AS constraint_name

    FROM sys.indexes i

    INNER JOIN sys.index_columns ic
        ON i.object_id = ic.object_id
        AND i.index_id = ic.index_id

    WHERE i.is_primary_key = 1
) pk
    ON pk.object_id = c.object_id
    AND pk.column_id = c.column_id

LEFT JOIN sys.foreign_key_columns fk
    ON fk.parent_object_id = c.object_id
    AND fk.parent_column_id = c.column_id

LEFT JOIN sys.tables rt
    ON fk.referenced_object_id = rt.object_id

LEFT JOIN sys.columns rc
    ON fk.referenced_object_id = rc.object_id
    AND fk.referenced_column_id = rc.column_id

LEFT JOIN sys.foreign_keys fkref
    ON fk.constraint_object_id = fkref.object_id

{whereClause}

ORDER BY
    s.name,
    t.name,
    c.column_id;
";

        if (!string.IsNullOrWhiteSpace(schema))
        {
            var p = cmd.CreateParameter();
            p.ParameterName = "@Schema";
            p.Value = schema;
            cmd.Parameters.Add(p);
        }

        using var reader =
            await cmd.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            var schemaName =
                reader["SchemaName"]?.ToString() ?? "";

            var tableName =
                reader["TableName"]?.ToString() ?? "";

            var table = metadata.FirstOrDefault(m =>
                m.Schema == schemaName &&
                m.Name == tableName);

            if (table == null)
            {
                table = new TableMetadata
                {
                    Schema = schemaName,
                    Name = tableName,
                    Columns = new List<ColumnMetadata>()
                };

                metadata.Add(table);
            }

            table.Columns.Add(new ColumnMetadata
            {
                Name = reader["ColumnName"]?.ToString() ?? "",
                SqlType = reader["DataType"]?.ToString() ?? "",
                MaxLength = reader["MaxLength"]?.ToString(),
                Precision = reader["Precision"]?.ToString(),
                Scale = reader["Scale"]?.ToString(),
                IsNullable =
                    reader["IsNullable"]?.ToString() == "YES",
                IsIdentity =
                    reader["IsIdentity"]?.ToString() == "YES",
                IsComputed =
                    reader["IsComputed"]?.ToString() == "YES",
                Collation =
                    reader["Collation"]?.ToString(),
                DefaultValue =
                    reader["DefaultValue"]?.ToString(),
                IsPrimaryKey =
                    reader["IsPrimaryKey"]?.ToString() == "YES",
                PrimaryKeyName =
                    reader["PrimaryKeyName"]?.ToString(),
                ForeignTable =
                    reader["ForeignTable"]?.ToString(),
                ForeignColumn =
                    reader["ForeignColumn"]?.ToString(),
                ForeignKeyName =
                    reader["ForeignKeyName"]?.ToString(),
                FK_DeleteAction =
                    reader["FK_DeleteAction"]?.ToString(),
                FK_UpdateAction =
                    reader["FK_UpdateAction"]?.ToString(),
                FK_IsDisabled =
                    reader["FK_IsDisabled"]?.ToString() == "1",
                FK_IsNotTrusted =
                    reader["FK_IsNotTrusted"]?.ToString() == "1"
            });
        }

        return metadata;
    }

}
