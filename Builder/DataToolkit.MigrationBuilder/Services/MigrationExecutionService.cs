using DataToolkit.BulkTransfer.Abstractions;
using DataToolkit.Library;
using DataToolkit.Library.UnitOfWorkLayer;
using DataToolkit.MigrationBuilder.Infrastructure.Migration;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text.RegularExpressions;

namespace DataToolkit.MigrationBuilder.Services;

public sealed class MigrationExecutionService
{
    private readonly ArtifactDiscoveryService _discovery;
    private readonly DatabaseRuntimeService _db;
    private readonly IBulkTransferEngine _bulk;
    private readonly IConfiguration _configuration;

    public MigrationExecutionService(
        ArtifactDiscoveryService discovery,
        DatabaseRuntimeService db,
        IBulkTransferEngine bulk,
        IConfiguration configuration)
    {
        _discovery = discovery;
        _db = db;
        _configuration = configuration;
        _bulk = bulk;

    }

    public async Task<List<MigrationTableResult>> ExecuteAsync(
        IUnitOfWork source,
        IUnitOfWork target,
        string ddlFolder,
        string sqlFolder,
        string approvedPath = "",
        bool isDelete = false)
    {
        List<MigrationTableResult> migrationResult = new();

        foreach (MigrationArtifact artifact in _discovery.Discover(ddlFolder, sqlFolder))
        {
            try
            {

            string? ddl = await File.ReadAllTextAsync(artifact.DdlFile);

            await _db.ExecuteDdlAsync(target, ddl);

            List<TableMetadata> sourceTable =
                await _db.GetMetadataAsync(
                    source,
                    artifact.Schema,
                    artifact.Table);

            List<TableMetadata> artifactTable =
                await _db.GetMetadataAsync(
                    target,
                    artifact.Schema,
                    $"{artifact.Prefix}_{artifact.Table}");

            List<TableMetadata> destinationTable =
                await _db.GetMetadataAsync(
                    target,
                    artifact.Schema,
                    artifact.Table);


            // Validar tablas requeridas por llaves foráneas.
            var foreignKeys = destinationTable
                .Single()
                .Columns
                .Where(c => !string.IsNullOrWhiteSpace(c.ForeignKeyName))
                .ToList();

            foreach (var column in foreignKeys)
            {
                long count = (await target.Sql.FromSqlAsync<long>(
                    $"SELECT COUNT(*) FROM [{destinationTable.Single().Schema}].[{column.ForeignTable}]"))
                    .Single();

                if (artifact.SqlFile is not null)
                    if (count == 0)
                    {
                        // Si no existe script SQL únicamente se crea la tabla.
                        throw new InvalidOperationException(
                        $"❌ No es posible migrar [{destinationTable.Single().Name}] porque la tabla [{column.ForeignTable}] está vacía. " +
                        $"Column: [{column.Name}] - FK [{column.ForeignKeyName}].");
                    }
            }

            // Validar tabla destino.
            if (isDelete)
            {
                await _db.ExecuteDdlAsync(
                    target,
                    $"DELETE FROM [{artifact.Schema}].[{artifact.Table}];");
            }
            else
            {
                long count = (await target.Sql.FromSqlAsync<long>(
                    $"SELECT COUNT(*) FROM [{artifact.Schema}].[{artifact.Table}]"))
                    .Single();

                if (count > 0)
                {
                    throw new InvalidOperationException(
                        $"❌ La tabla destino [{artifact.Schema}].[{artifact.Table}] contiene {count} registros. Debe vaciarla antes de ejecutar la migración.");
                }
            }

            long stgCount = 0;
            long destinationCount = 0;

            if (artifact.SqlFile is not null)
            { 
                string? sql = await File.ReadAllTextAsync(artifact.SqlFile);
                sql = RemoveComments(sql);
                SqlScriptValidator.Validate(sql);

                await using SqlConnection sourceConnection =
                    new(_configuration.GetConnectionString("Source"));

                await using SqlConnection targetConnection =
                    new(_configuration.GetConnectionString("Target"));

                await using SqlConnection targetReadConnection =
                    new(_configuration.GetConnectionString("Target"));

                await sourceConnection.OpenAsync();
                await targetConnection.OpenAsync();
                await targetReadConnection.OpenAsync();

                // Origen -> STG
                await _bulk.TransferAsync(
                    sourceConnection,
                    targetConnection,
                    sql,
                    artifactTable.Single());

                // STG -> Tabla Final
                string insertSql =
                    BuildInsert(
                        destinationTable.Single(),
                        false);

                string columnList =
                    BuildColumnList(
                        destinationTable.Single());

                columnList =
                    $"SELECT {columnList} FROM [{artifact.Schema}].[{artifact.Prefix}_{artifact.Table}]";

                string sqlInstruction =
                    $"{insertSql}{Environment.NewLine}{columnList}";

                await target.Sql.ExecuteAsync(sqlInstruction);

                // Validar cantidad de registros migrados.
                stgCount = (await target.Sql.FromSqlAsync<long>(
                    $"SELECT COUNT(*) FROM [{artifact.Schema}].[{artifact.Prefix}_{artifact.Table}]"))
                    .Single();

                destinationCount = (await target.Sql.FromSqlAsync<long>(
                    $"SELECT COUNT(*) FROM [{artifact.Schema}].[{artifact.Table}]"))
                    .Single();

                if (stgCount != destinationCount)
                {
                    throw new InvalidOperationException(
                        $"❌ La migración de [{artifact.Table}] finalizó con inconsistencias. STG: {stgCount} registro(s). Destino: {destinationCount} registro(s).");
                }
            }

            // Todas las migraciones finalizaron correctamente.
            // Aprobar los artefactos moviéndolos a la carpeta Approved.
            MoveFile(
                artifact.DdlFile,
                Path.Combine(
                    approvedPath,
                    "DDL",
                    Path.GetFileName(artifact.DdlFile)));

            if (artifact.SqlFile is not null)
                MoveFile(
                    artifact.SqlFile!,
                    Path.Combine(
                        approvedPath,
                        "EXTRACTION",
                        Path.GetFileName(artifact.SqlFile)));
            

            if (artifact.SqlFile is null)
            {
                migrationResult.Add(new MigrationTableResult
                {
                    Table = artifact.Table,
                    StagingRows = stgCount,
                    DestinationRows = destinationCount,
                    Success = true,
                    Message = $"✅ Ejecución completada correctamente. Sin extracción de datos."
                });
            }
            else
            {
                migrationResult.Add(new MigrationTableResult
                {
                    Table = artifact.Table,
                    StagingRows = stgCount,
                    DestinationRows = destinationCount,
                    Success = true,
                    Message = $"✅ Migración completada correctamente. {destinationCount} registro(s) migrados."
                });
            }
            }
            catch (Exception ex)
            {
                migrationResult.Add(new MigrationTableResult
                {
                    Table = artifact.Table,
                    StagingRows = 0,
                    DestinationRows = 0,
                    Success = false,
                    Message = $"❌ La ejecucion finalizo con inconsistencias [{artifact.Schema}].[{artifact.Table}]." + ex.Message //+ ex.InnerException.Message.ToString ()

                });
            }

        }

        return migrationResult;
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
        TableMetadata metadata, bool addParams = false)
    {
        var columns = metadata.Columns
            .Where(c => !c.IsIdentity)
            .Where(c => !c.IsComputed)
            .Select(c => $"[{c.Name}]");

        var parameters = metadata.Columns
            .Where(c => !c.IsIdentity)
            .Where(c => !c.IsComputed)
            .Select(c => $"@{c.Name}");

        if (addParams)
        {
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
        else
        {
            return $"""
INSERT INTO [{metadata.Schema}].[{metadata.Name}]
(
    {string.Join(", ", columns)}
)
""";
        }
    }

    private static string RemoveComments(string sql)
    {
        return Regex.Replace(
            sql,
            @"/\*.*?\*/",
            string.Empty,
            RegexOptions.Singleline);
    }

    private static void MoveFile(
        string sourceFile,
        string destinationFile)
    {
        if (!File.Exists(sourceFile))
        {
            return;
        }

        Directory.CreateDirectory(
            Path.GetDirectoryName(destinationFile)!);

        if (File.Exists(destinationFile))
        {
            File.Delete(destinationFile);
        }

        File.Move(sourceFile, destinationFile);
    }


}