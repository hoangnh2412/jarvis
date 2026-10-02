using Sample;
using Sample.AuthorizationDemo;
using Sample.Extensions;
using Sample.Health;
using Sample.Multitenancy;
using Sample.Persistence;
using Sample.Telemetry;
using Microsoft.EntityFrameworkCore;
using Platform.ORM.EntityFramework;
using Platform.ORM.Dapper;
using Platform.Multitenancy.EntityFramework;
using Npgsql;
using Platform.Mvc;
using Platform.Mvc.ExceptionHandling;
using Platform.Swashbuckle;
using Asp.Versioning;
using Platform.OpenTelemetry.Abstractions;
using Platform.OpenTelemetry.Extensions;
using Platform.OpenTelemetry.DDD.Extensions;
using Platform.DDD.Domain.Services;
using Platform.DDD.Domain;
using Platform.Authentication;
using Platform.Multitenancy;
using Platform.Mvc.ApplicationBuilders;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Platform.HealthChecks;
using Serilog;
using StackExchange.Redis;
using OpenTelemetry.Trace;
using Platform.BlobStoring.Extensions;
using Platform.Caching.Extensions;
using Platform.Caching.Redis;
using Platform.Caching.Redis.Extensions;
using Module.Notifications.Extensions;
using Platform.Modules.Notifications.Redis.Extensions;
using Platform.Realtime.Extensions;
using Platform.Realtime.SignalR.Extensions;
using Sample.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, _, configuration) => configuration.ReadFrom.Configuration(context.Configuration).Enrich.FromLogContext(), writeToProviders: true);
builder.Services
    .AddPlatformOpenTelemetry(builder.Configuration, services =>
    {
        // User/tenant - cả log + trace
        services.AddUserContextTelemetryEnrichment<CurrentUserInfo, CurrentTenantInfo>();

        // Case 1 - chỉ log
        services.AddScoped<IEnrichLogService, SampleLogOnlyEnrichmentService>();

        // Case 2 - can trace
        services.AddScoped<IEnrichTraceService, SampleTraceOnlyEnrichmentService>();

        // Case 3 - cả log and trace (IEnrichmentSource)
        services.AddScoped<IEnrichmentSource, SampleSharedEnrichmentSource>();
    })
    .ConfigureResource()
    .ConfigureLogging()
    .ConfigureTrace(options =>
    {
        options
            .AddEntityFrameworkCoreInstrumentation()
            .AddPlatformCachingDistributedRedisInstrumentation(builder.Configuration)
            .AddPlatformCachingMemoryInvalidationRedisInstrumentation();
    })
    .ConfigureMetric();

builder.AddCoreJson();
builder.AddCoreCors();
builder.AddCoreDomain();
builder.AddCurrentUser<CurrentUserInfo>();
builder.AddCurrentTenant<CurrentTenantInfo>();
builder.Services.TryAddSingleton<ICurrentUserStore<CurrentUserInfo>, SampleCurrentUserStore>();
builder.Services.TryAddSingleton<ICurrentTenantStore<CurrentTenantInfo>, SampleCurrentTenantStore>();
builder.AddCoreWebApi();

builder.AddPlatformCaching()
    .UseRedisDistributedCache()
    .UseRedisMemoryCacheInvalidation();

builder.AddSampleSettings();

builder.AddCoreBlobStoring();

builder.AddEntityFramework();
builder.AddMultitenancyEntityFramework();
builder.AddOrmDapper(options =>
{
    options.ConnectionStringName = "MasterDbContext";
    options.CreateConnection = connectionString => new NpgsqlConnection(connectionString);
});
builder.Services.AddScoped<SampleDapperReadSample>();
builder.AddSampleDbContext();
builder.AddMultitenancyEfTestHostedService();

// Redis (StackExchange) for Sample demos - same config shape as Cache:DistributedGroups:Redis:Default.
builder.Services.AddKeyedSingleton<IConnectionMultiplexer>("Default", (_, keyedService) =>
{
    var configuration = _.GetRequiredService<IConfiguration>();
    var redisConfig = configuration["Cache:DistributedGroups:Redis:Default:Configuration"];
    return ConnectionMultiplexer.Connect(redisConfig!);
});

builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(int.Parse(ApiVersions.MajorVersion), int.Parse(ApiVersions.MinorVersion));
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
}).AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV"; // Format: v1, v2
    options.SubstituteApiVersionInUrl = true;
});

builder.AddSampleAuthentication();
builder.AddSampleAuthorization();
builder.AddSampleWorkflow();

builder.AddNotificationModule();
builder.AddCoreRealtime()
    .UseSignalR()
    .UseRedisInboxStore()
    .AddNotificationAppServiceWithRealtime<CurrentUserInfo, CurrentTenantInfo>();

builder.AddCoreSwagger();

// builder.Services.AddHostedService<Worker>();

// builder.Services.AddHttpClient(SampleDogCeoApiHealthCheck.HttpClientName, client =>
// {
//     client.Timeout = TimeSpan.FromSeconds(5);
// });
// builder.Services.AddHttpClient(SampleArticArtworksApiHealthCheck.HttpClientName, client =>
// {
//     client.Timeout = TimeSpan.FromSeconds(5);
// });
// builder.AddHealthChecks();
// builder.AddSampleReadinessHealthChecks();

var app = builder.Build();
app.UseCoreSwagger();
app.UseHttpsRedirection();

app.UseCoreSpa();

app.UseCoreCors(); 

app.UseAuthentication();
app.UseAuthorization();
app.UseSampleWorkflow();

// Demo headers for OTEL trace enrichment (request: send x-demo-request; response: x-demo-response).
app.UseMiddleware<SampleOtlpDemoHeadersMiddleware>();

// Custom metrics (OTLP via Platform ConfigureMetric -> same meter name "Sample").
app.UseMiddleware<SampleApiCallMetricsMiddleware>();

app.UseSerilogRequestLogging();

app.UsePlatformOpenTelemetry();
app.UseCoreMiddleware<ApiResponseWrapperMiddleware>();
app.MapControllers();
app.MapRealtimeHub<CurrentUserInfo, CurrentTenantInfo>();
// app.UseHealthChecks();


app.EnsureMigrateDb<IMasterUnitOfWork>();
app.EnsureMigrateTemplateDb<TenantDbContext>((config, options) => options.UseNpgsql(config.GetConnectionString("TenantDbContext")));
await app.SeedSampleAuthorizationAsync();
app.Run();