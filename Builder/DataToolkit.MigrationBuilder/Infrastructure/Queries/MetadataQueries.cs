using DataToolkit.Library.Fluent;

namespace DataToolkit.MigrationBuilder.Infrastructure.Queries;

public static class MetadataQueries
{
    public static (string Sql, object Parameters) GetMetadata(
        string? schema,
        string? table)
    {
        IFluentQuery query = new FluentQuery()
            .Select(
                "s.name AS SchemaName",
                "t.name AS TableName",
                "c.name AS ColumnName",
                "ty.name AS DataType",
                """
                CASE
                    WHEN c.max_length = -1 THEN 'MAX'
                    WHEN ty.name IN ('nvarchar','nchar')
                        THEN CAST(c.max_length / 2 AS VARCHAR(10))
                    ELSE CAST(c.max_length AS VARCHAR(10))
                END AS MaxLength
                """,
                "c.precision AS Precision",
                "c.scale AS Scale",

                """
                CASE
                    WHEN c.is_nullable = 1 THEN 'YES'
                    ELSE 'NO'
                END AS IsNullable
                """,
                """
                CASE
                    WHEN c.is_identity = 1 THEN 'YES'
                    ELSE 'NO'
                END AS IsIdentity
                """,
                """
                CASE
                    WHEN c.is_computed = 1 THEN 'YES'
                    ELSE 'NO'
                END AS IsComputed
                """,
                "c.collation_name AS Collation",
                "dc.definition AS DefaultValue",

                """
                CASE
                    WHEN pk.column_id IS NOT NULL THEN 'YES'
                    ELSE 'NO'
                END AS IsPrimaryKey
                """,
                "pk.constraint_name AS PrimaryKeyName",
                "rt.name AS ForeignTable",
                "rc.name AS ForeignColumn",
                "fkref.name AS ForeignKeyName",
                "fkref.delete_referential_action_desc AS FK_DeleteAction",
                "fkref.update_referential_action_desc AS FK_UpdateAction",
                "fkref.is_disabled AS FK_IsDisabled",
                "fkref.is_not_trusted AS FK_IsNotTrusted")

            .From("sys.schemas s")
            .InnerJoin(
                "sys.tables t",
                "t.schema_id = s.schema_id")
            .InnerJoin(
                "sys.columns c",
                "c.object_id = t.object_id")
            .InnerJoin(
                "sys.types ty",
                "c.user_type_id = ty.user_type_id")
            .LeftJoin(
                "sys.default_constraints dc",
                "c.default_object_id = dc.object_id");
        // Pendiente:
        // LEFT JOIN (SELECT...) pk
        query
            .LeftJoin(
                "sys.foreign_key_columns fk",
                "fk.parent_object_id = c.object_id AND fk.parent_column_id = c.column_id")
            .LeftJoin(
                "sys.tables rt",
                "fk.referenced_object_id = rt.object_id")
            .LeftJoin(
                "sys.columns rc",
                "fk.referenced_object_id = rc.object_id AND fk.referenced_column_id = rc.column_id")
            .LeftJoin(
                "sys.foreign_keys fkref",
                "fk.constraint_object_id = fkref.object_id");

        if (!string.IsNullOrWhiteSpace(schema))
        {
            query.Where("s.name = @Schema", new { Schema = schema });
        }

        if (!string.IsNullOrWhiteSpace(table))
        {
            query.And("t.name = @Table", new { Table = table });
        }
        return query
            .OrderBy(
                "s.name",
                "t.name",
                "c.column_id")
            .Build();
    }
}