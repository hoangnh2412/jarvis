// {Product}.Host/Program.cs hoặc Infrastructure/DependencyInjection

using Jarvis.BlobStoring.Extensions;
// using Jarvis.BlobStoring.MinIO.Extensions;
// using Jarvis.BlobStoring.AwsS3.Extensions;

var builder = WebApplication.CreateBuilder(args);

// FileSystem (UseFileSystem được gọi bên trong)
builder.AddCoreBlobStoring();

// Tùy chọn — cần package + config
// builder.AddCoreBlobStoring()
//     .UseMinIO()
//     .UseAwsS3();

// Chỉ định default rõ ràng:
// builder.AddCoreBlobStoring(o => o.DefaultProvider = nameof(BlobStoringType.MinIO))
//     .UseMinIO();

var app = builder.Build();
