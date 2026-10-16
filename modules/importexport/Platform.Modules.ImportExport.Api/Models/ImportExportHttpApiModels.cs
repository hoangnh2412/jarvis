using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace Platform.Modules.ImportExport.API.Models;

/// <summary>HTTP multipart body cho <c>POST /api/v1/excel/import</c>.</summary>
public sealed class ImportApiRequest
{
    [Required]
    public required IFormFile File { get; init; }

    [Required]
    public required string TemplateCode { get; init; }

    /// <summary>Entity registry key for persist routing — required when using generic import API.</summary>
    public string? EntityKey { get; init; }
}

/// <summary>HTTP JSON body cho <c>POST /api/v1/excel/export</c>.</summary>
public sealed class ExportApiRequest
{
    [Required]
    public required string TemplateCode { get; init; }

    [Required]
    public required List<Dictionary<string, JsonElement>> Data { get; init; }

    public string? FileName { get; init; }
}

public sealed class CreateTemplateForm
{
    public required string Code { get; init; }
    public required string Name { get; init; }
    public required IFormFile File { get; init; }
    public required string Mappings { get; init; }
    public int HeaderRow { get; init; } = 1;
    public int StartRow { get; init; } = 2;
    public int? RowCount { get; init; }
}

public sealed class UpdateTemplateForm
{
    public string? Name { get; init; }
    public IFormFile? File { get; init; }
    public string? Mappings { get; init; }
    public int? HeaderRow { get; init; }
    public int? StartRow { get; init; }
    public int? RowCount { get; init; }
}

public sealed class UpdateTemplateConfigurationForm
{
    public IFormFile? File { get; init; }
    public string? Mappings { get; init; }
    public int? HeaderRow { get; init; }
    public int? StartRow { get; init; }
    public int? RowCount { get; init; }
}

public sealed class UpdateTemplateMetadataForm
{
    public required string Name { get; init; }
}

public sealed record ImportApiResponse(
    IReadOnlyList<SheetImportResult> Sheets,
    int ProcessedRowCount,
    int FailedRowCount,
    int SavedCount);

public sealed class TemplateApiResponse
{
    public required string Code { get; init; }
    public required string Name { get; init; }
    public required string FileName { get; init; }
    public string? Location { get; init; }
    public int HeaderRow { get; init; }
    public int StartRow { get; init; }
    public int? RowCount { get; init; }
    public string? SheetName { get; init; }
    public required IReadOnlyList<ColumnMapping> Mappings { get; init; }
    public IReadOnlyList<SheetTemplate> Sheets { get; init; } = [];
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }
}

/// <summary>
/// File download as JSON so host middleware can wrap the payload when needed.
/// </summary>
public sealed class FileDownloadResponse
{
    public required string FileName { get; init; }
    public required string ContentType { get; init; }
    public required string ContentBase64 { get; init; }
}
