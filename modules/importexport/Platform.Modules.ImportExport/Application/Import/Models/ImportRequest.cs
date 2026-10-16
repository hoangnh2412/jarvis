using Platform.Modules.ImportExport.Domain.Enums;

namespace Platform.Modules.ImportExport.Application.Import.Models;

/// <summary>
/// Public import input — dùng chung cho HTTP API và gọi DLL.
/// Consumer chỉ cần file + <see cref="TemplateCode"/>; engine tự load layout từ store.
/// </summary>
/// <remarks>
/// Không truyền <c>Mappings</c> / <c>HeaderRow</c> / <c>StartRow</c> — các giá trị đó lấy từ template đã đăng ký.
/// </remarks>
public sealed class ImportRequest
{
    /// <summary>
    /// Mã template đã tạo qua <see cref="ITemplateService"/> (ví dụ <c>CUSTOMER_IMPORT</c>).
    /// </summary>
    public required string TemplateCode { get; init; }

    /// <summary>Stream file Excel/CSV cần import.</summary>
    public required Stream FileStream { get; init; }

    /// <summary>
    /// Tên file gốc (dùng kiểm tra extension và log), ví dụ <c>customers.xlsx</c>.
    /// </summary>
    public required string FileName { get; init; }

    /// <summary>
    /// GĐ1 chỉ hỗ trợ <see cref="Enums.ExecutionMode.Sync"/>.
    /// </summary>
    public ExecutionMode ExecutionMode { get; init; } = ExecutionMode.Sync;

    /// <summary>
    /// <see cref="ImportExecutionPhase.ValidateOnly"/> skips persist; default full import.
    /// </summary>
    public ImportExecutionPhase Phase { get; init; } = ImportExecutionPhase.Import;

    /// <summary>
    /// Entity registry key for persist routing (e.g. <c>Student</c>).
    /// Host module services MUST set when calling import.
    /// </summary>
    public string? EntityKey { get; init; }
}
