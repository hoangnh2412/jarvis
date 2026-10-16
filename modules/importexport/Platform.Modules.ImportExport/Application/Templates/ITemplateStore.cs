using Platform.Modules.ImportExport.Domain.Models;

namespace Platform.Modules.ImportExport.Application.Templates;

/// <summary>
/// <b>Bắt buộc consumer đăng ký</b> (custom hoặc adapter có sẵn) — nơi lưu metadata + file template.
/// Hỗ trợ mọi kiểu: Postgres+disk, folder server, Azure Blob, AWS S3, …
/// </summary>
/// <remarks>
/// Chọn một:
/// <list type="bullet">
/// <item><c>AddLlaExcelTemplates(config)</c> — Postgres + file Location</item>
/// <item><c>AddLlaExcelFolderTemplates()</c> — JSON + xlsx trên disk</item>
/// <item><c>services.AddScoped&lt;ITemplateStore, MyAzureOrS3Store&gt;()</c> — cloud/custom</item>
/// </list>
/// Không đăng ký → resolve <see cref="ITemplateService"/> / import-export theo <c>TemplateCode</c> sẽ fail lúc runtime.
/// </remarks>
public interface ITemplateStore
{
    Task<TemplateDefinition?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);

    Task SaveAsync(TemplateDefinition template, CancellationToken cancellationToken = default);

    Task DeleteAsync(string code, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TemplateDefinition>> ListAsync(CancellationToken cancellationToken = default);
}
