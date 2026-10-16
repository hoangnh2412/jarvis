using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Platform.BlobStoring.Extensions;
using Platform.BlobStoring.SharePoint.Extensions;
using Platform.BlobStoring.Synology.Extensions;
using Platform.Modules.Files.API.Extensions;
using Platform.Modules.Files.Extensions;

namespace Sample.Extensions;

/// <summary>
/// Sample host wiring: blob providers (when configured) + Files module HTTP API.
/// </summary>
public static class SampleFilesExtensions
{
    private const string SharePointSection = "BlobStoring:SharePoint";
    private const string SynologySection = "BlobStoring:Synology";

    /// <summary>
    /// Registers blob storing and the Files module with optional SharePoint/Synology providers.
    /// </summary>
    public static IHostApplicationBuilder AddSampleFiles(this IHostApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        var blobStoring = builder.AddCoreBlobStoring();
        if (IsSharePointConfigured(builder.Configuration))
            blobStoring.UseSharePoint();
        if (IsSynologyConfigured(builder.Configuration))
            blobStoring.UseSynology();

        var files = builder.AddCoreFiles();
        files.UseFileSystemTree();
        if (IsSharePointConfigured(builder.Configuration))
            files.UseSharePointTree();
        if (IsSynologyConfigured(builder.Configuration))
            files.UseSynologyTree();

        files.UseHttpApi();

        return builder;
    }

    private static bool IsSharePointConfigured(IConfiguration configuration)
    {
        var section = configuration.GetSection(SharePointSection);
        return !string.IsNullOrWhiteSpace(section["TenantId"])
            && !string.IsNullOrWhiteSpace(section["ClientId"])
            && !string.IsNullOrWhiteSpace(section["ClientSecret"])
            && !string.IsNullOrWhiteSpace(section["DriveId"]);
    }

    private static bool IsSynologyConfigured(IConfiguration configuration)
    {
        var section = configuration.GetSection(SynologySection);
        return !string.IsNullOrWhiteSpace(section["Host"])
            && !string.IsNullOrWhiteSpace(section["Username"])
            && !string.IsNullOrWhiteSpace(section["Password"]);
    }
}
