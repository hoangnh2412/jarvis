using Platform.Modules.ImportExport.Domain.Models;
using Platform.Modules.ImportExport.Domain.Enums;

namespace Platform.Modules.ImportExport.Application.Import.Models;

/// <summary>
/// Payload đã resolve template — chỉ dùng trong pipeline (<see cref="Interfaces.IImportProcessor"/>).
/// Consumer API/DLL hãy dùng <see cref="ImportRequest"/>, không tự tạo type này.
/// </summary>
public sealed class ImportWork
{
    /// <summary>Mã template đã resolve (copy từ store).</summary>
    public string? TemplateCode { get; init; }

    /// <summary>Stream file import.</summary>
    public required Stream FileStream { get; init; }

    /// <summary>Tên file import (extension / log).</summary>
    public required string FileName { get; init; }

    /// <summary>Layout mapping / sheet plans lấy từ template store.</summary>
    public required TemplateConfiguration Template { get; init; }

    /// <summary>Template file name from store — used for sheetName validation (.csv vs .xlsx).</summary>
    public string? TemplateFileName { get; init; }

    /// <summary>Validate-only skips <see cref="IImportDataPersister"/>.</summary>
    public ImportExecutionPhase Phase { get; init; } = ImportExecutionPhase.Import;

    /// <summary>Registry key for module persist routing — from <see cref="ImportRequest"/> (host).</summary>
    public string? EntityKey { get; init; }
}
