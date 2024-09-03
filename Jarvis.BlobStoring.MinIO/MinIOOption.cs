namespace Jarvis.BlobStoring.MinIO;

public class MinIOOption
{
    public string Endpoint { get; set; } = string.Empty;
    public string AccessKey { get; set; } = string.Empty;
    public string SecretKey { get; set; } = string.Empty;
    public string Region { get; set; } = string.Empty;
    public string SessionToken { get; set; } = string.Empty;
    public string BucketName { get; set; } = string.Empty;
    public bool UseSsl { get; set; }
}