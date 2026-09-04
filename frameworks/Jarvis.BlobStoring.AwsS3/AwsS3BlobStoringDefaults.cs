namespace Jarvis.BlobStoring.AwsS3;

public static class AwsS3BlobStoringDefaults
{
    public const string ConfigurationSection = "AwsS3";

    public const int DefaultAutoSelectPriority = 20;

    public static bool IsEnabled(AwsS3BlobOptions options) => !string.IsNullOrWhiteSpace(options.Region) && !string.IsNullOrWhiteSpace(options.BucketName);

    public static int ResolveAutoSelectPriority(AwsS3BlobOptions options) => options.AutoSelectPriority > 0 ? options.AutoSelectPriority : DefaultAutoSelectPriority;
}
