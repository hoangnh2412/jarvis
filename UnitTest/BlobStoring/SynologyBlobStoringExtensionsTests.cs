using Platform.BlobStoring;
using Platform.BlobStoring.Extensions;
using Platform.BlobStoring.Hosting;
using Platform.BlobStoring.Synology;
using Platform.BlobStoring.Synology.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace UnitTest.BlobStoring;

public class SynologyBlobStoringExtensionsTests
{
    [Fact]
    public void UseSynology_Registers_Keyed_Blob_And_Folder_Services()
    {
        var hostBuilder = Host.CreateApplicationBuilder();
        hostBuilder.AddCoreBlobStoring(o => o.DefaultProvider = nameof(BlobStoringType.Synology))
            .UseSynology(sy =>
            {
                sy.Host = "nas.local";
                sy.Username = "user";
                sy.Password = "pass";
                sy.RootPath = "/team-folders/ERP";
                sy.AutoSelectPriority = 23;
            });

        using var host = hostBuilder.Build();

        var keyed = host.Services.GetRequiredKeyedService<IBlobStoringService>(
            nameof(BlobStoringType.Synology));
        var folder = host.Services.GetRequiredService<ISynologyFolderService>();
        var options = host.Services.GetRequiredService<IOptions<SynologyBlobOptions>>().Value;
        var registry = host.Services.GetRequiredService<BlobStoringProviderRegistry>();

        Assert.IsType<SynologyBlobStoringService>(keyed);
        Assert.Same(keyed, folder);
        Assert.Equal("nas.local", options.Host);
        Assert.Equal("/team-folders/ERP", options.RootPath);
        Assert.Equal(
            nameof(BlobStoringType.Synology),
            registry.ResolveDefaultProviderKey(null));
    }

    [Fact]
    public void UseSynology_Configure_Overrides_RootPath()
    {
        var hostBuilder = Host.CreateApplicationBuilder();
        hostBuilder.AddCoreBlobStoring()
            .UseSynology(sy =>
            {
                sy.Host = "nas.local";
                sy.Username = "user";
                sy.Password = "pass";
                sy.RootPath = "/mydrive/docs";
            });

        using var host = hostBuilder.Build();
        var options = host.Services.GetRequiredService<IOptions<SynologyBlobOptions>>().Value;

        Assert.Equal("/mydrive/docs", options.RootPath);
    }

    [Fact]
    public void UseSynology_Throws_When_Credentials_Missing()
    {
        var hostBuilder = Host.CreateApplicationBuilder();

        var ex = Assert.Throws<InvalidOperationException>(() =>
            hostBuilder.AddCoreBlobStoring().UseSynology());

        Assert.Contains("Host, Username, and Password are required", ex.Message, StringComparison.Ordinal);
    }
}
