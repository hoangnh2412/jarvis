using Platform.BlobStoring;
using Platform.BlobStoring.Extensions;
using Platform.BlobStoring.Hosting;
using Platform.BlobStoring.SharePoint;
using Platform.BlobStoring.SharePoint.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace UnitTest.BlobStoring;

public class SharePointBlobStoringExtensionsTests
{
    [Fact]
    public void UseSharePoint_Registers_Keyed_Blob_And_Folder_Services()
    {
        var hostBuilder = Host.CreateApplicationBuilder();
        hostBuilder.AddCoreBlobStoring(o => o.DefaultProvider = nameof(BlobStoringType.SharePoint))
            .UseSharePoint(sp =>
            {
                sp.TenantId = "tenant";
                sp.ClientId = "client";
                sp.ClientSecret = "secret";
                sp.DriveId = "drive-id";
                sp.AutoSelectPriority = 25;
            });

        using var host = hostBuilder.Build();

        var keyed = host.Services.GetRequiredKeyedService<IBlobStoringService>(
            nameof(BlobStoringType.SharePoint));
        var folder = host.Services.GetRequiredService<ISharePointFolderService>();
        var options = host.Services.GetRequiredService<IOptions<SharePointBlobOptions>>().Value;
        var registry = host.Services.GetRequiredService<BlobStoringProviderRegistry>();

        Assert.IsType<SharePointBlobStoringService>(keyed);
        Assert.Same(keyed, folder);
        Assert.Equal("tenant", options.TenantId);
        Assert.Equal("drive-id", options.DriveId);
        Assert.Equal(
            nameof(BlobStoringType.SharePoint),
            registry.ResolveDefaultProviderKey(null));
    }

    [Fact]
    public void UseSharePoint_Configure_Overrides_ViewLinkScope()
    {
        var hostBuilder = Host.CreateApplicationBuilder();
        hostBuilder.AddCoreBlobStoring()
            .UseSharePoint(sp =>
            {
                sp.TenantId = "tenant";
                sp.ClientId = "client";
                sp.ClientSecret = "secret";
                sp.DriveId = "drive-id";
                sp.ViewLinkScope = "anonymous";
            });

        using var host = hostBuilder.Build();
        var options = host.Services.GetRequiredService<IOptions<SharePointBlobOptions>>().Value;

        Assert.Equal("anonymous", options.ViewLinkScope);
    }
}
