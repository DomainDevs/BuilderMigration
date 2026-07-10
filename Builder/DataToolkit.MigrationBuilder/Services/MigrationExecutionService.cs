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
            string? ddl = await File.ReadAllTextAsync(artifact.DdlFile); //Leer DDL
            await _db.ExecuteDdlAsync(target, ddl); //Compilar tabla destino DDL

            //Limpiar tabla destino
            await _db.ExecuteDdlAsync(target,
                $"DELETE FROM {artifact.Schema}.{artifact.Table};");

            List<TableMetadata> sourceTable //ORIGEN: tabla real
                = await _db.GetMetadataAsync(source, artifact.Schema, artifact.Table);
            List<TableMetadata> artifactTable //DESTINO: DDL, tabla creada
                = await _db.GetMetadataAsync(target, artifact.Schema, $"{artifact.Prefix}_{artifact.Table}");
            List<TableMetadata> destinationTable //DESTINO: tabla real
                = await _db.GetMetadataAsync(target, artifact.Schema, artifact.Table);

            string columns = BuildColumnList(sourceTable.Single());

            //Si no hay archivo de extraccion, solo compila la DDL
            if (artifact.SqlFile is null)
                continue;

            //Leer el script de extraccion
            string? sql = await File.ReadAllTextAsync(artifact.SqlFile);
            artifact.SqlFile = null;

            //Validar si el script es valido para ejecutarse
            SqlScriptValidator.Validate(sql);

            //Crear un cursor de extraccion datos desde el origen (con el script).
            var rows = await _db.ExecuteExtractionAsync(source, sql);

            // TODO:
            // AutoMap sourceTable -> artifactTable
            // Insert rows into WF/STG/HM
            // Execute final load (Parameterized SQL Statement).
            var insertSql =
                BuildInsert(
                    artifactTable.Single());

            foreach (var row in rows)
            {
                await target.Sql.ExecuteAsync(
                    insertSql,
                    row);
            }

            // TODO:
            // AutoMap artifactTable -> destinationTable
            // Insert rows into destinationTable
            // Execute final load (Parameterized SQL Statement).
            insertSql = BuildInsert(destinationTable.Single());
            

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