using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Platform.Modules.ImportExport.Domain.Constants;

namespace Platform.Modules.ImportExport.API.Controllers;

/// <summary>
/// HTTP API schema discovery cho template UI (entity registry).
/// </summary>
[ApiVersionNeutral]
[ApiController]
[Route("api/v{version:apiVersion}/importexport/entities")]
public sealed class ImportExportEntitiesController(
    IImportExportSchemaService schemaService,
    ILogger<ImportExportEntitiesController> logger)
    : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<ImportExportEntitySummary>>(StatusCodes.Status200OK)]
    public ActionResult<IReadOnlyList<ImportExportEntitySummary>> List()
    {
        logger.LogDebug(EngineLogMessages.SchemaListRequested);
        return Ok(schemaService.ListEntities());
    }

    [HttpGet("{key}/fields")]
    [ProducesResponseType<IReadOnlyList<ImportExportFieldSchema>>(StatusCodes.Status200OK)]
    public ActionResult<IReadOnlyList<ImportExportFieldSchema>> Fields(string key)
    {
        logger.LogDebug(EngineLogMessages.SchemaFieldsRequested, key);
        return Ok(schemaService.GetFields(key));
    }
}
