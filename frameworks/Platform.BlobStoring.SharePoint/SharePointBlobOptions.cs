// Namespace gom các kiểu cấu hình và dịch vụ của provider SharePoint, tránh trùng tên với provider lưu trữ khác.
namespace Platform.BlobStoring.SharePoint;

/// <summary>
/// Chứa cấu hình của provider SharePoint Online được bind từ section
/// <c>BlobStoring:SharePoint</c> theo Options pattern của .NET.
/// </summary>
/// <remarks>
/// <para>
/// Configuration Binder maps properties from <c>appsettings.json</c>, environment variables,
/// or other configuration sources. <c>UseSharePoint()</c> requires TenantId, ClientId,
/// ClientSecret, and DriveId (non-empty). Host still owns correct Drive ID values
/// (e.g. from DB); package does not resolve Drive from site/library.
/// </para>
/// <example>
/// Ví dụ cấu hình JSON:
/// <code>
/// {
///   "BlobStoring": {
///     "DefaultProvider": "SharePoint",
///     "SharePoint": {
///       "TenantId": "...",
///       "ClientId": "...",
///       "ClientSecret": "...",
///       "DriveId": "...",
///       "AutoSelectPriority": 25
///     }
///   }
/// }
/// </code>
/// </example>
/// <para>
/// Ánh xạ khái niệm: <c>DriveId</c> = Document Library; tham số contract <c>bucket</c>
/// được hiện thực thành <c>folder</c> dưới library root; <c>fileName</c> = path file tương đối trong folder đó.
/// </para>
/// </remarks>
public class SharePointBlobOptions
{
    /// <summary>
    /// Mã tenant Microsoft Entra ID chứa ứng dụng và SharePoint Online cần truy cập.
    /// </summary>
    // Khởi tạo chuỗi rỗng để property luôn non-null trước khi Configuration Binder gán giá trị.
    public string TenantId { get; set; } = string.Empty;

    /// <summary>
    /// Client ID (Application ID) của app registration dùng để xác thực với Microsoft Graph.
    /// </summary>
    // Setter cho phép Options pattern bind và ghi đè giá trị từ IConfiguration.
    public string ClientId { get; set; } = string.Empty;

    /// <summary>
    /// Client secret của app registration dùng trong luồng client credentials.
    /// </summary>
    // Đây là thông tin nhạy cảm; production nên cấp qua secret store hoặc biến môi trường thay vì commit vào source.
    public string ClientSecret { get; set; } = string.Empty;

    /// <summary>
    /// Drive ID của Document Library trên SharePoint (Microsoft Graph).
    /// Host phải cung cấp đúng giá trị (config hoặc DB); package không resolve từ site/library.
    /// </summary>
    public string DriveId { get; set; } = string.Empty;

    /// <summary>
    /// Scope truyền cho thao tác <c>createLink</c> của
    /// <see cref="IBlobStoringService.ViewAsync"/>: <c>organization</c> (mặc định)
    /// hoặc <c>anonymous</c>. Tham số <c>expireTime</c> của contract không map sang TTL Graph.
    /// </summary>
    // Scope organization giới hạn link trong tenant; anonymous chỉ hoạt động khi policy SharePoint cho phép.
    public string ViewLinkScope { get; set; } = "organization";

    /// <summary>
    /// Độ ưu tiên khi registry tự chọn provider vì <c>DefaultProvider</c> để trống;
    /// giá trị <c>0</c> yêu cầu dùng mặc định tích hợp là <c>25</c>.
    /// </summary>
    // int mặc định bằng 0; hàm ResolveAutoSelectPriority chịu trách nhiệm đổi sang giá trị mặc định hợp lệ.
    public int AutoSelectPriority { get; set; }
}
