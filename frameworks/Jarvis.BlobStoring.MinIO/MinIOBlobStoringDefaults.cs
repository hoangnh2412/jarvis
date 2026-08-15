namespace Jarvis.BlobStoring.MinIO;

public static class MinIOBlobStoringDefaults
{
    public const string ConfigurationSection = "MinIO";

    public const int DefaultAutoSelectPriority = 30;

    public static bool IsEnabled(MinIOBlobStoringOption options) => !string.IsNullOrWhiteSpace(options.Endpoint);

    public static int ResolveAutoSelectPriority(MinIOBlobStoringOption options) => options.AutoSelectPriority > 0 ? options.AutoSelectPriority : DefaultAutoSelectPriority;
}
