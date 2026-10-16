using Asp.Versioning;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Platform.BlobStoring;
using Platform.BlobStoring.Extensions;
using Platform.Modules.Files.API.Extensions;
using Platform.Modules.Files.Extensions;

namespace UnitTest.Files.Helpers;

/// <summary>Minimal in-memory host exposing Files HTTP API (FileSystem provider).</summary>
public sealed class FilesTestServer : IAsyncDisposable
{
    private readonly WebApplication _app;
    private readonly string _rootPath;

    public HttpClient Client { get; }

    public string RootPath => _rootPath;

    private FilesTestServer(WebApplication app, HttpClient client, string rootPath)
    {
        _app = app;
        Client = client;
        _rootPath = rootPath;
    }

    public static async Task<FilesTestServer> CreateAsync(string? rootPath = null)
    {
        rootPath ??= Path.Combine(Path.GetTempPath(), "platform-files-http", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(rootPath);

        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            EnvironmentName = Environments.Development,
        });
        builder.WebHost.UseTestServer();

        builder.AddCoreBlobStoring(o => o.DefaultProvider = nameof(BlobStoringType.FileSystem))
            .UseFileSystem(fs => fs.RootPath = rootPath);

        builder.AddCoreFiles()
            .UseFileSystemTree()
            .UseHttpApi();

        builder.Services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new ApiVersion(2, 0);
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ReportApiVersions = true;
        });

        var app = builder.Build();
        app.MapControllers();
        await app.StartAsync().ConfigureAwait(false);

        return new FilesTestServer(app, app.GetTestClient(), rootPath);
    }

    public async ValueTask DisposeAsync()
    {
        Client.Dispose();
        await _app.StopAsync().ConfigureAwait(false);
        await _app.DisposeAsync().ConfigureAwait(false);

        if (Directory.Exists(_rootPath))
            Directory.Delete(_rootPath, recursive: true);
    }
}
