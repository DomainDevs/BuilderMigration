using System.ComponentModel;

namespace DataToolkit.MigrationBuilder.Models;

public sealed class ExecutionRequest
{
    [DefaultValue(false)]
    public bool DeleteTargetData { get; set; } = false;
}
