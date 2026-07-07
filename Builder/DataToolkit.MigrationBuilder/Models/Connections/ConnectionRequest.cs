using System.ComponentModel;

namespace DataToolkit.MigrationBuilder.Models.Connections;

public class ConnectionRequest
{
    [DefaultValue("localhost")]
    public string Server { get; set; } = "";
    public string Database { get; set; } = "";
    public string User { get; set; } = "";
    public string Password { get; set; } = "";
}
