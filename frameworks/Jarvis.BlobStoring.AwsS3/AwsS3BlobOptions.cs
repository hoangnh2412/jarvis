// Jarvis.BlobStoring.AwsS3 — Options bound from BlobStoring:AwsS3.
namespace Jarvis.BlobStoring.AwsS3;

/// <summary>
/// AWS S3 provider settings under <c>BlobStoring:AwsS3</c>.
/// Missing <see cref="Region"/> or <see cref="BucketName"/> = disabled.
/// </summary>
/// <remarks>
/// <example>
/// <code>
/// {
///   "BlobStoring": {
///     "DefaultProvider": "AwsS3",
///     "AwsS3": {
///       "Region": "ap-southeast-1",
///       "BucketName": "my-app-uploads",
///       "AccessKey": "AKIA...",
///       "SecretKey": "...",
///       "AutoSelectPriority": 20
///     }
///   }
/// }
/// // Empty AccessKey/SecretKey → SDK may use instance profile / environment credentials.
/// </code>
/// </example>
/// </remarks>
public class AwsS3BlobOptions
{
    public string Region { get; set; } = string.Empty;

    public string BucketName { get; set; } = string.Empty;

    public string AccessKey { get; set; } = string.Empty;

    public string SecretKey { get; set; } = string.Empty;

    /// <summary>Auto-default priority when <c>DefaultProvider</c> is empty. 0 = use built-in default (20).</summary>
    public int AutoSelectPriority { get; set; }
}
