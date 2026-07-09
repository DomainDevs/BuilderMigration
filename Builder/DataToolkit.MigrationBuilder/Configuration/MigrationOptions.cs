using DataToolkit.Library.Common;

namespace DataToolkit.MigrationBuilder.Configuration;

public sealed class MigrationOptions
{
    public const string SectionName = "Migration";

    public string WorkFilePath { get; set; } = "";

    public string SqlOutputPath { get; set; } = "";

    public string MetadataOutPut { get; set; } = "";

    public string DdlPath { get; set; } = "";

    public string ExtractionPath { get; set; } = "";

    public string DefaultSchema { get; set; } = "dbo";
}