using Platform.BlobStoring;
using Platform.BlobStoring.SharePoint;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace UnitTest.BlobStoring;

public class SharePointBlobStoringServiceTests
{
    [Fact]
    public void Constructor_Does_Not_Create_Client_Immediately()
    {
        var service = new SharePointBlobStoringService(
            Options.Create(new SharePointBlobOptions
            {
                TenantId = "tenant",
                ClientId = "client",
                ClientSecret = "secret",
                DriveId = "drive-id"
            }),
            NullLogger<SharePointBlobStoringService>.Instance);

        Assert.NotNull(service);
        Assert.IsAssignableFrom<IBlobStoringService>(service);
        Assert.IsAssignableFrom<ISharePointFolderService>(service);
    }

    [Fact]
    public void Constructor_Accepts_Empty_Options_Without_Throwing()
    {
        var service = new SharePointBlobStoringService(
            Options.Create(new SharePointBlobOptions()),
            NullLogger<SharePointBlobStoringService>.Instance);

        Assert.NotNull(service);
    }

    [Fact]
    public async Task UploadAsync_Throws_On_Path_Traversal_Before_Client()
    {
        var service = new SharePointBlobStoringService(
            Options.Create(new SharePointBlobOptions
            {
                TenantId = "tenant",
                ClientId = "client",
                ClientSecret = "secret",
                DriveId = "drive-id"
            }),
            NullLogger<SharePointBlobStoringService>.Instance);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.UploadAsync("invoices", "../secret.pdf", [1, 2, 3]));
    }

    [Fact]
    public async Task UploadAsync_Throws_When_Bytes_Null()
    {
        var service = new SharePointBlobStoringService(
            Options.Create(new SharePointBlobOptions
            {
                TenantId = "tenant",
                ClientId = "client",
                ClientSecret = "secret",
                DriveId = "drive-id"
            }),
            NullLogger<SharePointBlobStoringService>.Instance);

        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            service.UploadAsync("invoices", "a.pdf", null!));
    }

    [Fact]
    public async Task DeleteBucketAsync_Empty_Bucket_Throws_Before_Client()
    {
        var service = new SharePointBlobStoringService(
            Options.Create(new SharePointBlobOptions
            {
                TenantId = "tenant",
                ClientId = "client",
                ClientSecret = "secret",
                DriveId = "drive-id"
            }),
            NullLogger<SharePointBlobStoringService>.Instance);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.DeleteBucketAsync(string.Empty));

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.DeleteBucketAsync(string.Empty, string.Empty));
    }
}
