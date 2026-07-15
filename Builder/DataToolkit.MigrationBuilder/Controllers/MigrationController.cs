using DataToolkit.BulkTransfer.Core;
using DataToolkit.Library.UnitOfWorkLayer;
using DataToolkit.MigrationBuilder.Helpers;
using DataToolkit.MigrationBuilder.Infrastructure.Connect;
using DataToolkit.MigrationBuilder.Services;
using DataToolkit.MigrationBuilder.Services.Migration;
using Microsoft.AspNetCore.Mvc;

namespace DataToolkit.MigrationBuilder.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MigrationController : ControllerBase
{
    private readonly MetadataService _metadataService;
    //private readonly MigrationMetadataService _migrationMetadataService;
    private readonly MigrationWorkFileService _workFileService;
    private readonly MigrationDdlGeneratorService _ddlGeneratorService;
    private readonly MigrationExtractionGeneratorService _migrationExtractionGeneratorService;

    private readonly MigrationExecutionService _migrationExecutionService;

    private readonly IUnitOfWork _source; //context db1
    private readonly IUnitOfWork _target; //context db2

    public MigrationController(
    MetadataService metadataService,
    //MigrationMetadataService migrationMetadataService,
    MigrationWorkFileService workFileService,
    MigrationDdlGeneratorService ddlGeneratorService,
    MigrationExtractionGeneratorService migrationExtractionGeneratorService,
    
    MigrationExecutionService migrationExecutionService,

    DataToolkitContext context)
    {
        _metadataService = metadataService;
        //_migrationMetadataService = migrationMetadataService;
        _workFileService = workFileService;
        _ddlGeneratorService = ddlGeneratorService;
        _migrationExtractionGeneratorService = migrationExtractionGeneratorService;
        _migrationExecutionService = migrationExecutionService;

        _source = context.Source;
        _target = context.Target;
    }


    /// <summary>
    /// Genera scripts SQL creacion tabla WF.
    /// </summary>
    [HttpPost("generate-ddl")]
    public async Task<IActionResult> GenerateDdl(
        [FromBody] CompareRequest request)
    {

        var metadataSource =
            await _metadataService.ExtractMetadataAsync(
                _source,
                request.Schema,
                request.Tables);

        var metadataTarget =
            await _metadataService.ExtractMetadataAsync(
                _target,
                request.Schema,
                request.Tables);

        await _ddlGeneratorService.GenerateDdlScriptsAsync(
            metadataSource, metadataTarget, request.ArtifactType);

        var outputPath = "";
        outputPath = _workFileService.pathconfigure();

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
                _source,
                request.Schema,
                request.Tables);

        var metadataTarget =
            await _metadataService.ExtractMetadataAsync(
                _target,
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
    /// Ejecuta la migración utilizando los artefactos generados
    /// (DDL + SQL + WorkFiles).
    /// </summary>
    [HttpPost("execute-migration")]
    public async Task<IActionResult> ExecuteMigration(
        [FromBody] ExecutionRequest request)
    {
        try
        {
            var ddlFolder = _workFileService.pathconfigureDDL();

            var sqlFolder = _workFileService.pathconfigureSQL();

            var approvedPath = _workFileService.pathApproved();

            List<MigrationTableResult> migrationResult =
                await _migrationExecutionService.ExecuteAsync(
                    _source,
                    _target,
                    ddlFolder,
                    sqlFolder,
                    approvedPath,
                    request.DeleteTargetData
                    );

            return Ok(new
            {
                Success = true,
                Message = "Migration executed successfully.",
                DdlFolder = ddlFolder,
                SqlFolder = sqlFolder,
                approvedFolder = approvedPath,
                Result = migrationResult
            });
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                Success = false,
                Message = ex.Message,
                Detail = ex.InnerException?.Message
            });
        }
    }

}
