using Platform.BlobStoring;
using Platform.BlobStoring.Synology;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace UnitTest.BlobStoring;

public class SynologyBlobStoringServiceTests
{
    [Fact]
    public void Constructor_Does_Not_Create_Client_Immediately()
    {
        var service = new SynologyBlobStoringService(
            Options.Create(new SynologyBlobOptions
            {
                Host = "nas.local",
                Username = "user",
                Password = "pass",
                RootPath = "/team-folders/ERP"
            }),
            NullLogger<SynologyBlobStoringService>.Instance);

        Assert.NotNull(service);
        Assert.IsAssignableFrom<IBlobStoringService>(service);
        Assert.IsAssignableFrom<ISynologyFolderService>(service);
    }

    [Fact]
    public void Constructor_Accepts_Empty_Options_Without_Throwing()
    {
        var service = new SynologyBlobStoringService(
            Options.Create(new SynologyBlobOptions()),
            NullLogger<SynologyBlobStoringService>.Instance);

        Assert.NotNull(service);
    }

    [Fact]
    public void Dispose_Is_Idempotent_Without_Creating_Client()
    {
        var service = new SynologyBlobStoringService(
            Options.Create(new SynologyBlobOptions
            {
                Host = "nas.local",
                Username = "user",
                Password = "pass"
            }),
            NullLogger<SynologyBlobStoringService>.Instance);

        service.Dispose();
        service.Dispose();
    }

    [Fact]
    public async Task UploadAsync_Throws_When_Bytes_Null()
    {
        var service = new SynologyBlobStoringService(
            Options.Create(new SynologyBlobOptions
            {
                Host = "nas.local",
                Username = "user",
                Password = "pass",
                RootPath = "/team-folders/ERP"
            }),
            NullLogger<SynologyBlobStoringService>.Instance);

        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            service.UploadAsync("bucket", "a.pdf", null!));
    }

    [Fact]
    public async Task UploadAsync_Throws_On_Path_Traversal_Before_Client()
    {
        var service = new SynologyBlobStoringService(
            Options.Create(new SynologyBlobOptions
            {
                Host = "nas.local",
                Username = "user",
                Password = "pass",
                RootPath = "/team-folders/ERP"
            }),
            NullLogger<SynologyBlobStoringService>.Instance);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.UploadAsync("bucket", "../a.pdf", [1, 2, 3]));
    }

    [Fact]
    public async Task DeleteFilesAsync_Empty_List_Does_Not_Throw()
    {
        var service = new SynologyBlobStoringService(
            Options.Create(new SynologyBlobOptions
            {
                Host = "nas.local",
                Username = "user",
                Password = "pass"
            }),
            NullLogger<SynologyBlobStoringService>.Instance);

        await service.DeleteFilesAsync("bucket", Array.Empty<string>());
        service.Dispose();
    }

    [Fact]
    public async Task CreateBucketAsync_Throws_On_Path_Traversal_In_Path_Before_Client()
    {
        var service = new SynologyBlobStoringService(
            Options.Create(new SynologyBlobOptions
            {
                Host = "nas.local",
                Username = "user",
                Password = "pass",
                RootPath = "/team-folders/ERP"
            }),
            NullLogger<SynologyBlobStoringService>.Instance);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CreateBucketAsync("invoices", "../escape"));

        service.Dispose();
    }

    [Fact]
    public async Task MoveAsync_Throws_On_Path_Traversal_In_Destination_Before_Client()
    {
        var service = new SynologyBlobStoringService(
            Options.Create(new SynologyBlobOptions
            {
                Host = "nas.local",
                Username = "user",
                Password = "pass",
                RootPath = "/team-folders/ERP"
            }),
            NullLogger<SynologyBlobStoringService>.Instance);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.MoveAsync("invoices", "2024/a.pdf", "../escape"));

        service.Dispose();
    }

    [Fact]
    public async Task MoveAsync_Throws_When_Moving_Into_Subfolder_Before_Client()
    {
        var service = new SynologyBlobStoringService(
            Options.Create(new SynologyBlobOptions
            {
                Host = "nas.local",
                Username = "user",
                Password = "pass",
                RootPath = "/team-folders/ERP"
            }),
            NullLogger<SynologyBlobStoringService>.Instance);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.MoveAsync("invoices", "2024", "2024/Q1"));

        service.Dispose();
    }

    [Fact]
    public async Task DeleteBucketAsync_Empty_Bucket_Throws_Before_Client()
    {
        var service = new SynologyBlobStoringService(
            Options.Create(new SynologyBlobOptions
            {
                Host = "nas.local",
                Username = "user",
                Password = "pass",
                RootPath = "/team-folders/ERP"
            }),
            NullLogger<SynologyBlobStoringService>.Instance);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.DeleteBucketAsync(string.Empty));

        service.Dispose();
    }
}
