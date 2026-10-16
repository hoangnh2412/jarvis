// Platform.BlobStoring.Synology — Options bound from BlobStoring:Synology.
namespace Platform.BlobStoring.Synology;

/// <summary>
/// Cấu hình provider Synology Drive bind từ <c>BlobStoring:Synology</c>.
/// </summary>
/// <remarks>
/// <para>
/// Host cung cấp Host / Username / Password hợp lệ. <c>UseSynology()</c> yêu cầu ba field này khác rỗng.
/// Production: Password qua User Secrets / env / vault.
/// </para>
/// <example>
/// <code>
/// {
///   "BlobStoring": {
///     "DefaultProvider": "Synology",
///     "Synology": {
///       "Host": "192.168.1.10",
///       "Port": 5001,
///       "UseHttps": true,
///       "Username": "svc-account",
///       "Password": "...",
///       "RootPath": "/team-folders/ERP",
///       "AutoSelectPriority": 23
///     }
///   }
/// }
/// </code>
/// </example>
/// <para>
/// Ánh xạ: <c>bucket</c> = folder dưới <see cref="RootPath"/>; <c>fileName</c> = object key tương đối.
/// </para>
/// </remarks>
public class SynologyBlobOptions
{
    public string Host { get; set; } = string.Empty;

    public int Port { get; set; }

    public bool UseHttps { get; set; } = true;

    public string Username { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    /// <summary>Drive root path, e.g. <c>/team-folders/ERP</c> or <c>/mydrive</c> (API path, not UI label).</summary>
    public string RootPath { get; set; } = string.Empty;

    /// <summary>Auto-default priority when <c>DefaultProvider</c> is empty. 0 = use built-in default (23).</summary>
    public int AutoSelectPriority { get; set; }
}
