using Platform.Caching.Extensions;
using Platform.Caching.Redis;
using Platform.DDD.Domain.Services;
using Platform.Host.Extensions;
using Platform.MultiOrg.Middleware;
using Platform.Mvc;
using Platform.Mvc.ApplicationBuilders;
using Platform.Mvc.ExceptionHandling;
using Platform.OpenTelemetry.Middleware;
using Platform.Swashbuckle;
using Platform.Tenants.Services;
using Sample.AuthorizationDemo;
using Sample.Extensions;
using Sample.Health;
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
builder.AddPlatformHost<SampleTenantDbContext, ISampleTenantUnitOfWork, SampleTenantDbContext>();
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
app.UseCoreMiddleware<MultiOrgMiddleware>();
app.UseAuthorization();

app.UseWorkflowEndpoints();

// Header demo cho OTEL (request: x-demo-request; response: x-demo-response).
app.UseCoreMiddleware<SampleOtlpDemoHeadersMiddleware>();

// Metric tùy chỉnh — cùng meter "Sample" qua ConfigureMetric.
app.UseCoreMiddleware<SampleApiCallMetricsMiddleware>();

app.UseSerilogRequestLogging();
app.UseCoreMiddleware<TraceEnrichmentMiddleware>();
app.UseCoreMiddleware<LogEnrichmentMiddleware>();
app.UseCoreMiddleware<ApiResponseWrapperMiddleware>();

app.MapControllers();
app.MapRealtimeEndpoints();
app.UseSampleHealthChecks();

await app.MigrateAndSeedAsync();

app.Run();
