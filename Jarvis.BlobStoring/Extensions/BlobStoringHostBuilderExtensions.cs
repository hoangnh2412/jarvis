// Jarvis.BlobStoring — Host DI: binds BlobStoring options, registers default IBlobStoringService.
using Jarvis.BlobStoring.Configuration;
using Jarvis.BlobStoring.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Jarvis.BlobStoring.Extensions;

/// <summary>
/// Registers Jarvis blob storing (FileSystem default; optional MinIO/S3 via package extensions).
/// </summary>
public static class BlobStoringHostBuilderExtensions
{
    /// <summary>
    /// Binds <see cref="JarvisBlobStoringOptions"/> and registers FileSystem + default <see cref="IBlobStoringService"/>.
    /// Call <c>UseMinIO()</c> / <c>UseAwsS3()</c> on the returned builder when those packages are referenced.
    /// </summary>
    /// <param name="builder">Host application builder.</param>
    /// <param name="configure">
    /// Optional delegate applied to options after configuration binding (same values as <c>services.Configure</c> and startup snapshot).
    /// </param>
    /// <returns>Fluent builder for optional <c>UseMinIO()</c> / <c>UseAwsS3()</c>.</returns>
    /// <remarks>
    /// <para><b>appsettings.json</b></para>
    /// <example>
    /// <code>
    /// {
    ///   "BlobStoring": {
    ///     "DefaultProvider": "",
    ///     "FileSystem": { "RootPath": "D:/data/blobs", "SubPath": "", "AutoSelectPriority": 10 },
    ///     "MinIO": { "Endpoint": "localhost:9000", "AccessKey": "...", "SecretKey": "...", "UseSsl": false, "AutoSelectPriority": 30 },
    ///     "AwsS3": { "Region": "ap-southeast-1", "BucketName": "my-bucket", "AccessKey": "", "SecretKey": "", "AutoSelectPriority": 20 }
    ///   }
    /// }
    /// </code>
    /// </example>
    /// <para><b>FileSystem only (local dev)</b></para>
    /// <example>
    /// <code>
    /// using Jarvis.BlobStoring.Extensions;
    ///
    /// var blob = builder.AddBlobStoring();
    ///
    /// public class FileService(IBlobStoringService blobs) { }
    /// </code>
    /// </example>
    /// <para><b>Override options in code (<paramref name="configure"/>)</b></para>
    /// <example>
    /// <code>
    /// using Jarvis.BlobStoring;
    ///
    /// builder.AddBlobStoring(options =>
    /// {
    ///     options.DefaultProvider = nameof(BlobStoringType.FileSystem);
    ///     options.FileSystem.RootPath = @"D:\uploads";
    ///     options.FileSystem.SubPath = "tenant-1";
    /// });
    /// </code>
    /// </example>
    /// <para><b>MinIO (reference Jarvis.BlobStoring.MinIO)</b></para>
    /// <example>
    /// <code>
    /// using Jarvis.BlobStoring;
    /// using Jarvis.BlobStoring.Extensions;
    /// using Jarvis.BlobStoring.MinIO.Extensions;
    ///
    /// builder.AddBlobStoring(o => o.DefaultProvider = nameof(BlobStoringType.MinIO))
    ///     .UseMinIO(minio =>
    ///     {
    ///         minio.Endpoint = "localhost:9000";
    ///         minio.AccessKey = "minioadmin";
    ///         minio.SecretKey = "minioadmin";
    ///     });
    /// </code>
    /// </example>
    /// <para>Empty <c>DefaultProvider</c> picks the registered provider with highest <c>AutoSelectPriority</c> from appsettings (typical: MinIO 30 &gt; AwsS3 20 &gt; FileSystem 10).</para>
    /// </remarks>
    public static BlobStoringBuilder AddCoreBlobStoring(
        this IHostApplicationBuilder builder,
        Action<JarvisBlobStoringOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.Services
            .AddOptions<JarvisBlobStoringOptions>()
            .BindConfiguration(JarvisBlobStoringOptions.SectionName);

        if (configure is not null)
            builder.Services.Configure(configure);

        var snapshot = new JarvisBlobStoringOptions();
        builder.Configuration.GetSection(JarvisBlobStoringOptions.SectionName).Bind(snapshot);
        configure?.Invoke(snapshot);

        builder.Services.GetOrAddProviderRegistry();

        builder.Services.TryAddSingleton(sp =>
        {
            var options = sp.GetRequiredService<IOptions<JarvisBlobStoringOptions>>().Value;
            var registry = sp.GetRequiredService<BlobStoringProviderRegistry>();
            var key = registry.ResolveDefaultProviderKey(options.DefaultProvider);
            return sp.GetRequiredKeyedService<IBlobStoringService>(key);
        });

        return new BlobStoringBuilder(builder, snapshot).UseFileSystem();
    }
}
