using DataToolkit.MigrationBuilder.Services;
using DataToolkit.MigrationBuilder.Services.Migration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace DataToolkit.MigrationBuilder.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MigrationController : ControllerBase
{
    private readonly MetadataService _metadataService;
    private readonly MigrationMetadataService _migrationMetadataService;
    private readonly MigrationWorkFileService _workFileService;
    private readonly MigrationDdlGeneratorService _ddlGeneratorService;
    private readonly MigrationExtractionGeneratorService _migrationExtractionGeneratorService;
    

    public MigrationController(
    MetadataService metadataService,
    MigrationMetadataService migrationMetadataService,
    MigrationWorkFileService workFileService,
    MigrationDdlGeneratorService ddlGeneratorService,
    MigrationExtractionGeneratorService migrationExtractionGeneratorService)
    {
        _metadataService = metadataService;
        _migrationMetadataService = migrationMetadataService;
        _workFileService = workFileService;
        _ddlGeneratorService = ddlGeneratorService;
        _migrationExtractionGeneratorService = migrationExtractionGeneratorService;
    }

    /// <summary>
    /// Compara metadata entre origen y destino.
    /// </summary>
    /*
    [HttpPost("compare")]
    public async Task<IActionResult> Compare(
        [FromBody] CompareRequest request)
    {
        if (request == null)
            return BadRequest("Request inválido.");

        var sourceMetadata =
            await _metadataService.ExtractMetadataAsync(
                request.SourceConnectionString,
                request.Schema,
                request.Tables);

        var targetMetadata =
            await _metadataService.ExtractMetadataAsync(
                request.TargetConnectionString,
                request.Schema,
                request.Tables);

        var differences =
            _migrationMetadataService.CompareMetadata(
                sourceMetadata,
                targetMetadata);

        return Ok(differences);
    }
    */

    /// <summary>
    /// Genera WorkFiles.
    /// </summary>
    [HttpPost("metadata-report")]
    public async Task<IActionResult> GenerateMetadataReport(
        [FromBody] CompareRequest request)
    {
        if (request == null)
            return BadRequest("Request inválido.");

        var sourceMetadata =
            await _metadataService.ExtractMetadataAsync(
                request.SourceConnectionString,
                request.Schema,
                request.Tables);

        var targetMetadata =
            await _metadataService.ExtractMetadataAsync(
                request.TargetConnectionString,
                request.Schema,
                request.Tables);

        var outputPath =
            Path.Combine(
                AppContext.BaseDirectory,
                "METADATA_OUTPUT");

        outputPath = _migrationMetadataService.pathconfigure(1);
        await _migrationMetadataService.GenerateWorkFilesAsync(
            sourceMetadata,
            targetMetadata,
            outputPath);

        return Ok(new
        {
            OutputPath = outputPath,
            FilesGenerated = true
        });
    }

    /// <summary>
    /// Genera WorkFiles de migración.
    /// </summary>
    /*
    [HttpPost("workfiles")]
    public async Task<IActionResult> GenerateWorkFiles(
        [FromBody] CompareRequest request)
    {
        if (request == null)
            return BadRequest("Request inválido.");

        var sourceMetadata =
            await _metadataService.ExtractMetadataAsync(
                request.SourceConnectionString,
                request.Schema,
                request.Tables);

        var targetMetadata =
            await _metadataService.ExtractMetadataAsync(
                request.TargetConnectionString,
                request.Schema,
                request.Tables);

        var outputPath =
            Path.Combine(
                AppContext.BaseDirectory,
                "WF_OUTPUT");
        outputPath = _workFileService.pathconfigure();

        await _workFileService.GenerateMigrationWorkFilesAsync(
            sourceMetadata,
            targetMetadata,
            request.ArtifactType,
            outputPath);

        return Ok(new
        {
            OutputPath = outputPath,
            WorkFilesGenerated = true
        });
    }
    */

    /// <summary>
    /// Genera scripts SQL creacion tabla WF.
    /// </summary>
    [HttpPost("generate-ddl")]
    public async Task<IActionResult> GenerateDdl(
        [FromBody] CompareRequest request)
    {

        var metadataSource =
            await _metadataService.ExtractMetadataAsync(
                request.SourceConnectionString,
                request.Schema,
                request.Tables);

        var metadataTarget =
            await _metadataService.ExtractMetadataAsync(
                request.TargetConnectionString,
                request.Schema,
                request.Tables);

        await _ddlGeneratorService.GenerateDdlScriptsAsync(
            metadataSource, metadataTarget, request.ArtifactType);

        var outputPath = "";
        outputPath = _workFileService.pathconfigure();

        //return Ok();
        return Ok(new
        {
            OutputPath = outputPath,
            WorkFilesGenerated = true
        });
    }


    /// <summary>
    /// Genera scripts SQL de extracción para poblar los Work Files.
    /// </summary>
    [HttpPost("generate-extraction")]
    public async Task<IActionResult> GenerateExtraction(
        [FromBody] CompareRequest request)
    {
        var metadataSource =
            await _metadataService.ExtractMetadataAsync(
                request.SourceConnectionString,
                request.Schema,
                request.Tables);

        var metadataTarget =
            await _metadataService.ExtractMetadataAsync(
                request.TargetConnectionString,
                request.Schema,
                request.Tables);

        await _migrationExtractionGeneratorService.GenerateExtractionScriptsAsync(
            metadataSource,
            metadataTarget,
            request.ArtifactType);

        var outputPath =
            _workFileService.pathconfigure();

        return Ok(new
        {
            OutputPath = outputPath,
            ExtractionScriptsGenerated = true
        });
    }

    /// <summary>
    /// Genera scripts SQL a partir de WorkFiles.
    /// </summary>
    /*
    [HttpPost("workFileToSql")]
    public async Task<IActionResult> GenerateSql(
        [FromBody] GenerateSqlRequest request)
    {
        await _sqlGeneratorService.GenerateSqlScriptsAsync(request);

        return Ok(new
        {
            ScriptsGenerated = true
        });
    }
    */

    /// <summary>
    /// Genera listado de dependecias de una tabla
    /// </summary>
    /*
    [HttpPost("table-dependencies")]
    public async Task<IActionResult> Dependencies(
        [FromBody] CompareRequest request, 
        [FromQuery] int level = 1)
    {
        var metadata =
            await _metadataService.ExtractMetadataAsync(
                request.SourceConnectionString,
                request.Schema,
                request.Tables);

        var tableName =
            request.Tables.FirstOrDefault();

        var dependencies =
            _dependencyService.GetDependencies(
                tableName!,
                metadata,
                level);

        return Ok(dependencies);
    }
    */

}