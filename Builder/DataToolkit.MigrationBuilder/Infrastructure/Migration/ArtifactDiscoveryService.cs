using System.Text.RegularExpressions;
using DataToolkit.MigrationBuilder.Models;

namespace DataToolkit.MigrationBuilder.Infrastructure.Migration;

public sealed class ArtifactDiscoveryService
{
    private static readonly Regex Rx = new(
        @"^(?<Order>\d+)_DDL_(?<Schema>[^.]+)\.(?<Prefix>WF|STG|HM)_(?<Table>.+)\.sql$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public IEnumerable<MigrationArtifact> Discover(string ddlFolder, string sqlFolder)
    {
        foreach (var file in Directory.GetFiles(ddlFolder, "*.sql").OrderBy(f => f))
        {
            var m = Rx.Match(Path.GetFileName(file));
            if (!m.Success) continue;

            string order = m.Groups["Order"].Value;
            string schema = m.Groups["Schema"].Value;
            string prefix = m.Groups["Prefix"].Value;
            string table = m.Groups["Table"].Value;

            string sqlArtefact = Path.Combine(sqlFolder, $"{order}_SQL_{schema}.{prefix}_{table}.sql");

            yield return new MigrationArtifact
            {
                Order = int.Parse(order),
                ArtifactKind = "DDL",
                Schema = schema,
                Prefix = prefix,
                Table = table,
                DdlFile = file,
                SqlFile = File.Exists(sqlArtefact) ? sqlArtefact : null
            };
        }
    }
}
