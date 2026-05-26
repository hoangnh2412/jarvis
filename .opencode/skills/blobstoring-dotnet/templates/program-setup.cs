// {Product}.Infrastructure/DependencyInjection/BlobStoringExtension.cs

using Jarvis.BlobStoring;
using Jarvis.BlobStoring.FileSystem;
using Jarvis.BlobStoring.MinIO;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace {Product}.Infrastructure.DependencyInjection;

public static class BlobStoringExtension
{
  public static IHostApplicationBuilder AddBlobStoring(this IHostApplicationBuilder builder)
  {
    builder.Services.Configure<FileSystemOption>(builder.Configuration.GetSection("FileSystem"));
    builder.Services.Configure<MinIOOption>(builder.Configuration.GetSection("MinIO"));

    builder.Services.AddKeyedSingleton<IBlobStoringService, FileSystemService>("FileSystem");
    builder.Services.AddKeyedSingleton<IBlobStoringService, MinioService>("MinIO");

    return builder;
  }
}
