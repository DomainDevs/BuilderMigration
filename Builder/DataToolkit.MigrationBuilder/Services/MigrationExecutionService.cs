using DataToolkit.BulkTransfer.Abstractions;
using DataToolkit.Library;
using DataToolkit.Library.UnitOfWorkLayer;
using DataToolkit.MigrationBuilder.Infrastructure.Migration;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
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
            sql = RemoveComments(sql);
            artifact.SqlFile = null;

            //Validar si el script es valido para ejecutarse
            SqlScriptValidator.Validate(sql);

            await using SqlConnection sourceConnection =
                new SqlConnection(_configuration.GetConnectionString("Source"));

            await using SqlConnection targetConnection =
                new SqlConnection(_configuration.GetConnectionString("Target"));

            await using SqlConnection targetReadConnection =
                new SqlConnection(_configuration.GetConnectionString("Target"));

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
            await _bulk.TransferAsync(
                targetReadConnection,
                targetConnection,
                artifactTable.Single(),
                destinationTable.Single());

            if (sourceConnection.State == ConnectionState.Open) sourceConnection.Close();
            if (targetConnection.State == ConnectionState.Open) targetConnection.Close();
            if (targetReadConnection.State == ConnectionState.Open) targetReadConnection.Close();

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

    private static string RemoveComments(string sql)
    {
        return Regex.Replace(
            sql,
            @"/\*.*?\*/",
            string.Empty,
            RegexOptions.Singleline);
    }

}