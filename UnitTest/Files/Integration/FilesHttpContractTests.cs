using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Asp.Versioning;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Platform.BlobStoring;
using Platform.BlobStoring.Extensions;
using Platform.Modules.Files;
using Platform.Modules.Files.API.Extensions;
using Platform.Modules.Files.Extensions;
using UnitTest.Files.Helpers;

namespace UnitTest.Files.Integration;

/// <summary>HTTP contract tests for <c>api/v2/files</c> via in-memory TestServer.</summary>
public class FilesHttpContractTests
{
    private const string FilesBase = "/api/v2/files";

    [Fact]
    public async Task Routes_Use_Files_Prefix_Not_Legacy_Blobs()
    {
        await using var server = await FilesTestServer.CreateAsync();

        var response = await server.Client.GetAsync($"{FilesBase}/contract-bucket");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Upload_With_Traversal_FileName_Returns_400_With_ErrorCode()
    {
        await using var server = await FilesTestServer.CreateAsync();

        using var content = CreateUploadContent("payload", "../escape.txt");
        var response = await server.Client.PostAsync(
            $"{FilesBase}/invoices/upload?fileName=..%2Fescape.txt",
            content);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var body = await ReadJsonAsync(response);
        Assert.Equal(FileErrorCode.InvalidPath, body.GetProperty("code").GetString());
        Assert.False(string.IsNullOrWhiteSpace(body.GetProperty("message").GetString()));
    }

    [Fact]
    public async Task Download_With_Traversal_FileName_Returns_400_With_ErrorCode()
    {
        await using var server = await FilesTestServer.CreateAsync();

        var response = await server.Client.GetAsync(
            $"{FilesBase}/invoices/download?fileName=..%2Fsecret.pdf");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var body = await ReadJsonAsync(response);
        Assert.Equal(FileErrorCode.InvalidPath, body.GetProperty("code").GetString());
    }

    [Fact]
    public async Task Delete_Bucket_With_Traversal_Prefix_Returns_400_With_ErrorCode()
    {
        await using var server = await FilesTestServer.CreateAsync();

        var response = await server.Client.DeleteAsync(
            $"{FilesBase}/invoices?prefix=..%2Fother");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var body = await ReadJsonAsync(response);
        Assert.Equal(FileErrorCode.InvalidPath, body.GetProperty("code").GetString());
    }

    [Fact]
    public async Task Get_Tree_Returns_200_For_FileSystem_With_Lazy_Children()
    {
        await using var server = await FilesTestServer.CreateAsync();

        var bucket = "tree-bucket";
        var createBucket = await server.Client.PostAsync($"{FilesBase}/{bucket}", content: null);
        Assert.Equal(HttpStatusCode.NoContent, createBucket.StatusCode);

        var emptyFolder = await server.Client.PostAsync(
            $"{FilesBase}/{bucket}?path=EmptyFolder",
            content: null);
        Assert.Equal(HttpStatusCode.NoContent, emptyFolder.StatusCode);

        var response = await server.Client.GetAsync($"{FilesBase}/tree?bucket={bucket}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var root = await ReadJsonAsync(response);
        Assert.True(root.GetProperty("isFolder").GetBoolean());
        Assert.Equal(string.Empty, root.GetProperty("path").GetString());

        var children = root.GetProperty("children");
        Assert.Equal(1, children.GetArrayLength());
        var emptyNode = children[0];
        Assert.Equal("EmptyFolder", emptyNode.GetProperty("name").GetString());
        Assert.True(emptyNode.GetProperty("isFolder").GetBoolean());
        Assert.Equal(0, emptyNode.GetProperty("children").GetArrayLength());

        var nested = await server.Client.GetAsync(
            $"{FilesBase}/tree?bucket={Uri.EscapeDataString(bucket)}&rootPath=EmptyFolder");
        nested.EnsureSuccessStatusCode();
        var nestedRoot = await ReadJsonAsync(nested);
        Assert.Equal("EmptyFolder", nestedRoot.GetProperty("name").GetString());
        Assert.Equal(0, nestedRoot.GetProperty("children").GetArrayLength());
    }

    [Fact]
    public async Task Get_Tree_Returns_501_When_Tree_Provider_Not_Registered()
    {
        var rootPath = Path.Combine(Path.GetTempPath(), "platform-files-http", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(rootPath);

        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            EnvironmentName = Environments.Development,
        });
        builder.WebHost.UseTestServer();
        builder.AddCoreBlobStoring(o => o.DefaultProvider = nameof(BlobStoringType.FileSystem))
            .UseFileSystem(fs => fs.RootPath = rootPath);
        builder.AddCoreFiles().UseHttpApi();
        builder.Services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new ApiVersion(2, 0);
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ReportApiVersions = true;
        });

        var app = builder.Build();
        app.MapControllers();
        await app.StartAsync();

        try
        {
            var client = app.GetTestClient();
            var response = await client.GetAsync($"{FilesBase}/tree");

            Assert.Equal(HttpStatusCode.NotImplemented, response.StatusCode);

            var body = await ReadJsonAsync(response);
            Assert.Equal(FileErrorCode.TreeNotSupported, body.GetProperty("code").GetString());
            Assert.Contains("FileSystem", body.GetProperty("message").GetString(), StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            await app.StopAsync();
            await app.DisposeAsync();
            if (Directory.Exists(rootPath))
                Directory.Delete(rootPath, recursive: true);
        }
    }

    [Fact]
    public async Task Upload_Without_Bucket_Route_Stores_At_Storage_Root()
    {
        await using var server = await FilesTestServer.CreateAsync();
        const string fileName = "no-bucket-route.txt";
        const string payload = "upload-without-bucket-route";

        using var uploadContent = CreateUploadContent(payload, "no-bucket-route.txt");
        var upload = await server.Client.PostAsync(
            $"{FilesBase}/upload?fileName={Uri.EscapeDataString(fileName)}",
            uploadContent);
        upload.EnsureSuccessStatusCode();

        var download = await server.Client.GetAsync(
            $"{FilesBase}/download?fileName={Uri.EscapeDataString(fileName)}");
        download.EnsureSuccessStatusCode();
        var bytes = await download.Content.ReadAsByteArrayAsync();
        Assert.Equal(payload, Encoding.UTF8.GetString(bytes));
    }

    [Fact]
    public async Task Upload_List_Download_Delete_File_HappyPath()
    {
        await using var server = await FilesTestServer.CreateAsync();
        const string bucket = "integration-bucket";
        const string fileName = "docs/readme.txt";
        const string payload = "integration-test-content";

        var create = await server.Client.PostAsync($"{FilesBase}/{bucket}", content: null);
        Assert.Equal(HttpStatusCode.NoContent, create.StatusCode);

        using var uploadContent = CreateUploadContent(payload, "readme.txt");
        var upload = await server.Client.PostAsync(
            $"{FilesBase}/{bucket}/upload?fileName={Uri.EscapeDataString(fileName)}",
            uploadContent);
        upload.EnsureSuccessStatusCode();

        var list = await server.Client.GetAsync($"{FilesBase}/{bucket}");
        list.EnsureSuccessStatusCode();
        var names = await ReadJsonStringArrayAsync(list);
        Assert.Contains(fileName, names);

        var download = await server.Client.GetAsync(
            $"{FilesBase}/{bucket}/download?fileName={Uri.EscapeDataString(fileName)}");
        download.EnsureSuccessStatusCode();
        var bytes = await download.Content.ReadAsByteArrayAsync();
        Assert.Equal(payload, Encoding.UTF8.GetString(bytes));

        var delete = await server.Client.DeleteAsync(
            $"{FilesBase}/{bucket}?fileName={Uri.EscapeDataString(fileName)}");
        Assert.Equal(HttpStatusCode.NoContent, delete.StatusCode);

        var listAfterDelete = await server.Client.GetAsync($"{FilesBase}/{bucket}");
        listAfterDelete.EnsureSuccessStatusCode();
        var namesAfterDelete = await ReadJsonStringArrayAsync(listAfterDelete);
        Assert.DoesNotContain(fileName, namesAfterDelete);
    }

    [Fact]
    public async Task Delete_Batch_With_Traversal_Path_Returns_400_With_ErrorCode()
    {
        await using var server = await FilesTestServer.CreateAsync();

        using var body = new StringContent("[\"ok.pdf\",\"../bad.pdf\"]", Encoding.UTF8, "application/json");
        var response = await server.Client.PostAsync($"{FilesBase}/invoices/delete-batch", body);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var json = await ReadJsonAsync(response);
        Assert.Equal(FileErrorCode.InvalidPath, json.GetProperty("code").GetString());
    }

    private static MultipartFormDataContent CreateUploadContent(string payload, string formFileName)
    {
        var content = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(Encoding.UTF8.GetBytes(payload));
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
        content.Add(fileContent, "file", formFileName);
        return content;
    }

    private static async Task<JsonElement> ReadJsonAsync(HttpResponseMessage response)
    {
        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        return doc.RootElement.Clone();
    }

    private static async Task<string[]> ReadJsonStringArrayAsync(HttpResponseMessage response)
    {
        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<string[]>(json) ?? [];
    }
}
