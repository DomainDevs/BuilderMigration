namespace DataToolkit.MigrationBuilder.Models.Metadata;

using DataToolkit.MigrationBuilder.Models.Metadata;

/// <summary>
/// Central engine responsible for merging source and target metadata.
/// Eliminates duplicate columns and resolves equivalent definitions.
/// </summary>
public sealed class MetadataMergeEngine
{
    public IReadOnlyList<ColumnMetadata> Merge(
        TableMetadata source,
        TableMetadata target)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(target);

        var result = new List<ColumnMetadata>();
        var added = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var column in source.Columns)
        {
            result.Add(Clone(column));
            added.Add(Normalize(column.Name));
        }

        foreach (var targetColumn in target.Columns)
        {
            var existing = result.FirstOrDefault(c =>
                AreEquivalent(c.Name, targetColumn.Name));

            if (existing is not null)
            {
                MergeDefinition(existing, targetColumn);
                continue;
            }

            if (added.Contains(Normalize(targetColumn.Name)))
                continue;

            result.Add(Clone(targetColumn));
            added.Add(Normalize(targetColumn.Name));
        }

        return result.ToList();

            //.OrderBy(c => c.OrdinalPosition).ToList();
    }

    public bool AreEquivalent(string left, string right)
    {
        if (string.Equals(left, right, StringComparison.OrdinalIgnoreCase))
            return true;

        var l = Normalize(left);
        var r = Normalize(right);

        if (l == r)
            return true;

        if (l.EndsWith("ID") && r.EndsWith("ID"))
            return l.Replace("ID", "") == r.Replace("ID", "");

        return false;
    }

    private static void MergeDefinition(
        ColumnMetadata source,
        ColumnMetadata target)
    {
        if (string.IsNullOrWhiteSpace(source.SqlType))
            source.SqlType = target.SqlType;

        if (string.IsNullOrWhiteSpace(source.MaxLength))
            source.MaxLength = target.MaxLength;

        if (string.IsNullOrWhiteSpace(source.Precision))
            source.Precision = target.Precision;

        if (string.IsNullOrWhiteSpace(source.Scale))
            source.Scale = target.Scale;

        source.IsNullable =
            source.IsNullable && target.IsNullable;
    }

    private static ColumnMetadata Clone(
        ColumnMetadata column)
    {
        return new ColumnMetadata
        {
            Name = column.Name,
            SqlType = column.SqlType,
            MaxLength = column.MaxLength,
            Precision = column.Precision,
            Scale = column.Scale,
            IsNullable = column.IsNullable,
            //OrdinalPosition = column.OrdinalPosition,
            DefaultValue = column.DefaultValue,
            IsIdentity = column.IsIdentity,
            IsPrimaryKey = column.IsPrimaryKey
        };
    }

    private static string Normalize(string name)
    {
        return name
            .Replace("_", "")
            .Replace("-", "")
            .Replace(" ", "")
            .Trim()
            .ToUpperInvariant();
    }
}
