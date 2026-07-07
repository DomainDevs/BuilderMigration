using DataToolkit.MigrationBuilder.Helpers;
using DataToolkit.MigrationBuilder.Models.Connections;
using System.ComponentModel;

namespace DataToolkit.MigrationBuilder.Models;

public sealed class CompareRequest
{
    /// <summary>
    /// Base de datos origen.
    /// </summary>
    public ConnectionRequest SourceConnectionString { get; set; }

    /// <summary> 
    /// Base de datos destino. 
    /// </summary>
    public ConnectionRequest TargetConnectionString { get; set; }

    /// <summary> 
    /// Esquema a comparar. 
    /// Null = todos los esquemas. 
    /// </summary>
    [DefaultValue("dbo")]
    public string? Schema { get; set; }

    /// <summary> 
    /// Define el tipo (WorkFile = 0 o Homologation = 1)
    /// Null = todos los esquemas. 
    /// </summary>
    [DefaultValue(ArtifactType.WorkFile)]
    public ArtifactType ArtifactType { get; set; } = ArtifactType.WorkFile;

    /// <summary>
    /// Tablas específicas a comparar. 
    /// Null o vacío = todas las tablas. 
    /// </summary>
    public List<string> Tables { get; set; } = [];
}


