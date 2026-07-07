namespace DataToolkit.MigrationBuilder.Helpers;

using DataToolkit.MigrationBuilder.Infrastructure;
using DataToolkit.MigrationBuilder.Models.Connections;
using Microsoft.Data.SqlClient;

public static class ConnectionStringHelper
{
    public static string Build(DatabaseConnectionOptions connection)
    {
        ArgumentNullException.ThrowIfNull(connection);

        var builder = new SqlConnectionStringBuilder
        {
            DataSource = connection.Servidor,
            InitialCatalog = connection.BaseDatos,
            UserID = connection.Usuario,
            Password = connection.Password,
            PersistSecurityInfo = connection.PersistSecurityInfo,
            TrustServerCertificate = true,
            Encrypt = false
        };

        return builder.ConnectionString;
    }
}