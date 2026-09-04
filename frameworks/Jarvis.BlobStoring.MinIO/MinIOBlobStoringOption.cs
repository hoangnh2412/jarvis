// Jarvis.BlobStoring.MinIO — Options bound from BlobStoring:MinIO.
namespace Jarvis.BlobStoring.MinIO;

/// <summary>
/// MinIO provider settings under <c>BlobStoring:MinIO</c>. Empty <see cref="Endpoint"/> = disabled.
/// </summary>
/// <remarks>
/// <example>
/// <code>
/// {
///   "BlobStoring": {
///     "DefaultProvider": "MinIO",
///     "MinIO": {
///       "Endpoint": "localhost:9000",
///       "AccessKey": "minioadmin",
///       "SecretKey": "minioadmin",
///       "UseSsl": false,
///       "AutoSelectPriority": 30
///     }
///   }
/// }
/// </code>
/// </example>
/// </remarks>
public class MinIOBlobStoringOption
{
    /// <summary>MinIO server host and port (no scheme), e.g. <c>minio.example.com:9000</c>.</summary>
    public string Endpoint { get; set; } = string.Empty;

    public string AccessKey { get; set; } = string.Empty;

    public string SecretKey { get; set; } = string.Empty;

    public string Region { get; set; } = string.Empty;

    public string SessionToken { get; set; } = string.Empty;

    public string BucketName { get; set; } = string.Empty;

    public bool UseSsl { get; set; }

    /// <summary>Auto-default priority when <c>DefaultProvider</c> is empty. 0 = use built-in default (30).</summary>
    public int AutoSelectPriority { get; set; }
}
