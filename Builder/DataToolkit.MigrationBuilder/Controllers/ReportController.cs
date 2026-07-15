using DataToolkit.Library.UnitOfWorkLayer;
using DataToolkit.MigrationBuilder.Infrastructure.Connect;
using DataToolkit.MigrationBuilder.Services;
using DataToolkit.MigrationBuilder.Services.Migration;
using Microsoft.AspNetCore.Mvc;

namespace DataToolkit.MigrationBuilder.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReportController : ControllerBase
{
    private readonly MetadataService _metadataService;
    private readonly MigrationMetadataService _migrationMetadataService;
    private readonly MigrationWorkFileService _workFileService;

    private readonly IUnitOfWork _source; //context db1
    private readonly IUnitOfWork _target; //context db2

    public ReportController(
    MetadataService metadataService,
    MigrationMetadataService migrationMetadataService,
    MigrationWorkFileService workFileService,
    DataToolkitContext context)
    {
        _metadataService = metadataService;
        _migrationMetadataService = migrationMetadataService;
        _workFileService = workFileService;
        _source = context.Source;
        _target = context.Target;
    }

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
                _source,
                request.Schema,
                request.Tables);

        var targetMetadata =
            await _metadataService.ExtractMetadataAsync(
                _target,
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

}
