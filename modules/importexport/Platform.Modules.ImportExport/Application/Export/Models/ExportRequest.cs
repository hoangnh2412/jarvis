using Platform.Modules.ImportExport.Domain.Enums;

namespace Platform.Modules.ImportExport.Application.Export.Models;

/// <summary>
/// Public export input — dùng chung cho HTTP API và gọi DLL.
/// Consumer chỉ cần <see cref="TemplateCode"/> + <see cref="Data"/>; engine tự load mappings và file template.
/// </summary>
/// <remarks>
/// Không truyền <c>Mappings</c> / <c>TemplateContent</c> / <c>HeaderRow</c> — lấy từ template đã đăng ký.
/// Khi template có file .xlsx, engine giữ style (SaveAsByTemplate + <c>{{rows.FieldName}}</c>).
/// </remarks>
public sealed class ExportRequest<T>
    where T : class
{
    /// <summary>
    /// Mã template đã tạo qua <see cref="ITemplateService"/> (ví dụ <c>CUSTOMER_EXPORT</c>).
    /// </summary>
    public required string TemplateCode { get; init; }

    /// <summary>
    /// Dữ liệu cần ghi ra Excel (POCO, dictionary, …). Key/property nên khớp <c>FieldName</c> của template.
    /// </summary>
    public required IEnumerable<T> Data { get; init; }

    /// <summary>
    /// Tên file tải về (optional). Bỏ trống → dùng <c>FileName</c> của template.
    /// </summary>
    public string? FileName { get; init; }

    /// <summary>
    /// GĐ1 chỉ hỗ trợ <see cref="Enums.ExecutionMode.Sync"/>.
    /// </summary>
    public ExecutionMode ExecutionMode { get; init; } = ExecutionMode.Sync;
}
