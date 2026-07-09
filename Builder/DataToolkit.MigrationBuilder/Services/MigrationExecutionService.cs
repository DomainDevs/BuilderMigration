using DataToolkit.Library.UnitOfWorkLayer;
using DataToolkit.MigrationBuilder.Infrastructure.Migration;

namespace DataToolkit.MigrationBuilder.Services;

public sealed class MigrationExecutionService
{
    private readonly ArtifactDiscoveryService _discovery;
    private readonly DatabaseRuntimeService _db;

    public MigrationExecutionService(
        ArtifactDiscoveryService discovery,
        DatabaseRuntimeService db)
    {
        _discovery = discovery;
        _db = db;
    }
    public async Task ExecuteAsync(
        IUnitOfWork source,
        IUnitOfWork target,
        string ddlFolder,
        string sqlFolder)
    {
        foreach (var artifact in _discovery.Discover(ddlFolder, sqlFolder))
        {
            var ddl = await File.ReadAllTextAsync(artifact.DdlFile);
            await _db.ExecuteDdlAsync(target, ddl);

            if (artifact.Prefix.Equals(
                    "WF",
                    StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            await _db.ExecuteDdlAsync(target, $"DELETE FROM {artifact.Schema}.{artifact.Table};");

            List<TableMetadata> sourceMeta //ORIGEN: tabla real
                = await _db.GetMetadataAsync(source, artifact.Schema, artifact.Table);
            List<TableMetadata> targetMeta //DESTINO: DDL, tabla creada
                = await _db.GetMetadataAsync(target, artifact.Schema, $"{artifact.Prefix}_{artifact.Table}");
            List<TableMetadata> targetMetaOutTable //DESTINO: tabla real
                = await _db.GetMetadataAsync(target, artifact.Schema, artifact.Table);

            //await _db.ExecuteDdlAsync(target, ddl); //Limpiar tabla destino

            var columns = BuildColumnList(sourceMeta.Single());

            string selectSql = $"""
SELECT
    {columns}
FROM [{artifact.Schema}].[{artifact.Table}]
""";

            var sql = artifact.SqlFile is null
                ? $"{selectSql} FROM {artifact.Schema}.{artifact.Table}"
                : await File.ReadAllTextAsync(artifact.SqlFile);

            SqlScriptValidator.Validate(sql);

            var rows = await _db.ExecuteExtractionAsync(source, sql);

            // TODO:
            // AutoMap sourceMeta -> targetMeta
            // Insert rows into WF/STG/HM
            // Execute final load
            var insertSql =
                BuildInsert(
                    targetMeta.Single());

            foreach (var row in rows)
            {
                await target.Sql.ExecuteAsync(
                    insertSql,
                    row);
            }

            insertSql = BuildInsert(targetMetaOutTable.Single());

            foreach (var row in rows)
            {
                await target.Sql.ExecuteAsync(
                    insertSql,
                    row);
            }

        }
    }


    //Armar columnas
    private static string BuildColumnList(
        TableMetadata metadata)
    {
        return string.Join(
            ", ",
            metadata.Columns
                .Where(c => !c.IsComputed)
                .Select(c => $"[{c.Name}]"));
    }

    //Insertar informacion
    private static string BuildInsert(
        TableMetadata metadata)
    {
        var columns = metadata.Columns
            .Where(c => !c.IsIdentity)
            .Where(c => !c.IsComputed)
            .Select(c => $"[{c.Name}]");

        var parameters = metadata.Columns
            .Where(c => !c.IsIdentity)
            .Where(c => !c.IsComputed)
            .Select(c => $"@{c.Name}");

        return $"""
INSERT INTO [{metadata.Schema}].[{metadata.Name}]
(
    {string.Join(", ", columns)}
)
VALUES
(
    {string.Join(", ", parameters)}
)
""";
    }

}