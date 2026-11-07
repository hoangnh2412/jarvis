using Platform.Caching.Extensions;
using Platform.Caching.Redis;
using Platform.DDD.Domain.Services;
using Platform.Host.Extensions;
using Platform.MultiOrg;
using Platform.Mvc;
using Platform.Mvc.ApplicationBuilders;
using Platform.Mvc.ExceptionHandling;
using Platform.OpenTelemetry.Extensions;
using Platform.Swashbuckle;
using Platform.Tenants.Services;
using Sample.AuthorizationDemo;
using Sample.Extensions;
using Sample.Health;
using Sample.ImportExport.Middleware;
using Sample.Persistence;
using Sample.Services;
using Sample.Telemetry;
using Serilog;

// Composition root của Sample.
// Extension theo nhóm nằm ở Sample/Extensions. Giữ đúng thứ tự middleware.

var builder = WebApplication.CreateBuilder(args);

builder.AddSampleSerilog();
builder.AddSampleOpenTelemetry();
builder.AddSampleWeb();
builder.AddPlatformCaching()
    .UseRedisDistributedCache()
    .UseRedisMemoryCacheInvalidation();
builder.AddSampleApplicationServices();
builder.AddSamplePersistence();
builder.AddSampleApiVersioning();
builder.AddSampleAuthentication();
builder.AddSampleAuthorization();
builder.Services.AddSingleton<ICurrentTenantStore<TenantCatalogProfile>, SampleTenantCatalogStore>();
builder.AddPlatformHost<TenantDbContext, ITenantUnitOfWork, SampleAuthorizationDbContext>();
builder.AddSampleWorkflow();
builder.AddSampleNotifications();
builder.AddCoreSwagger();
builder.AddSampleHealthChecks();

var app = builder.Build();

app.UseCoreSwagger();

app.UseHttpsRedirection();
app.UseCoreSpa();
app.UseCoreCors();

app.UseAuthentication();
app.UseMultiOrg(ignoreStartWithPaths: ["/hubs/", "/sse/", "/elsa/"]);
app.UseAuthorization();

app.UseWorkflowEndpoints();

// Header demo cho OTEL (request: x-demo-request; response: x-demo-response).
app.UseMiddleware<SampleOtlpDemoHeadersMiddleware>();

// Metric tùy chỉnh — cùng meter "Sample" qua ConfigureMetric.
app.UseMiddleware<SampleApiCallMetricsMiddleware>();

app.UseSerilogRequestLogging();
app.UsePlatformOpenTelemetry();
app.UseCoreMiddleware<ApiResponseWrapperMiddleware>();
app.UseMiddleware<ImportExportExceptionMiddleware>();

app.MapControllers();
app.MapRealtimeEndpoints();
app.UseSampleHealthChecks();

await app.MigrateAndSeedAsync();

app.Run();
