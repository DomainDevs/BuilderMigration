using DataToolkit.Library;
using DataToolkit.Library.Engine.Abstractions;
using DataToolkit.Library.UnitOfWorkLayer;
using DataToolkit.MigrationBuilder.Services;

namespace DataToolkit.MigrationBuilder.Infrastructure.Migration;

public sealed class DatabaseRuntimeService
{
    private readonly MetadataService _metadata;
    private readonly ISqlScriptExecutor _scriptExecutor;
    public DatabaseRuntimeService(
        MetadataService metadata,
        ISqlScriptExecutor scriptExecutor)
    { 
        _metadata = metadata;
        _scriptExecutor = scriptExecutor;
    }

    public Task ExecuteDdlAsync(IUnitOfWork uow, string ddlSql)
        => _scriptExecutor.ExecuteAsync(uow, ddlSql);

    public Task<List<TableMetadata>> GetMetadataAsync(
        IUnitOfWork uow, string schema, string table)
        => _metadata.ExtractMetadataAsync(uow, schema, new() { table });

    public Task<IEnumerable<IDictionary<string, object>>> ExecuteExtractionAsync(
        IUnitOfWork uow, string sql)
        => uow.Sql.FromSqlDictionaryAsync(sql);
}
