using DataToolkit.Library.Connections;
using DataToolkit.Library.UnitOfWorkLayer;

namespace DataToolkit.MigrationBuilder.Infrastructure.Connect;

public sealed class DataToolkitContext
{
    public IUnitOfWork Source { get; }

    public IUnitOfWork Target { get; }

    public DataToolkitContext(
        IDbConnectionFactory factory)
    {
        Source = new UnitOfWork(factory, "Source");
        Target = new UnitOfWork(factory, "Target");
    }
}