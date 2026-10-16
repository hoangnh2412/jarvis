using Platform.Modules.ImportExport.Domain.Constants;
using Platform.Modules.ImportExport.Domain.Models;

namespace Platform.Modules.ImportExport.Application.Export.Models;

/// <summary>
/// Payload đã resolve template — chỉ dùng trong pipeline (<see cref="Interfaces.IExportProcessor"/>).
/// Consumer API/DLL hãy dùng <see cref="ExportRequest{T}"/>, không tự tạo type này.
/// </summary>
public sealed class ExportWork<T>
    where T : class
{
    /// <summary>Dữ liệu cần ghi.</summary>
    public required IEnumerable<T> Data { get; init; }

    /// <summary>Column mappings từ template.</summary>
    public required IReadOnlyCollection<ColumnMapping> Mappings { get; init; }

    /// <summary>Tên sheet đích (null → sheet mặc định của engine).</summary>
    public string? SheetName { get; init; }

    /// <summary>Tên file output đã quyết định (sau khi merge optional override).</summary>
    public string FileName { get; init; } = EngineMessages.DefaultExportFileName;

    /// <summary>
    /// Bytes file template .xlsx. Có giá trị → SaveAsByTemplate (giữ style).
    /// Null/empty → SaveAs plain workbook.
    /// </summary>
    public byte[]? TemplateContent { get; init; }

    /// <summary>Dòng header trên template (1-based).</summary>
    public int HeaderRow { get; init; } = 1;

    /// <summary>Dòng bắt đầu data / placeholder (1-based).</summary>
    public int StartRow { get; init; } = 2;

    /// <summary>Giới hạn số bản ghi export (nullable). Chỉ lấy N phần tử đầu từ <see cref="Data"/>.</summary>
    public int? RowCount { get; init; }
}