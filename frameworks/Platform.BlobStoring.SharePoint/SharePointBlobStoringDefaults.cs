namespace Platform.BlobStoring.SharePoint;

/// <summary>
/// Constants and default values for the SharePoint blob storing provider.
/// </summary>
public static class SharePointBlobStoringDefaults
{
    public const string ConfigurationSection = "SharePoint";

    public const int DefaultAutoSelectPriority = 25;

    public static bool IsEnabled(SharePointBlobOptions options) =>
        !string.IsNullOrWhiteSpace(options.TenantId)
        && !string.IsNullOrWhiteSpace(options.ClientId)
        && !string.IsNullOrWhiteSpace(options.ClientSecret)
        && !string.IsNullOrWhiteSpace(options.DriveId);

    public static int ResolveAutoSelectPriority(SharePointBlobOptions options) =>
        options.AutoSelectPriority > 0 ? options.AutoSelectPriority : DefaultAutoSelectPriority;
}
