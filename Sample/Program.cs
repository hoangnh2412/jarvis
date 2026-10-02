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
using Sample.Workflows;
using Platform.Workflow.Configuration;
using Platform.Workflow.Extensions;
using Platform.Modules.ElsaWorkflow.Api.Extensions;
using Platform.Modules.ElsaWorkflow.EntityFramework.Extensions;
using Platform.Modules.ElsaWorkflow.Extensions;
using Elsa.Persistence.EFCore.Modules.Management;
using Elsa.Persistence.EFCore.Modules.Runtime;

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

// Workflow (Elsa) — bật/tắt bằng Elsa:Enabled (mặc định true).
// Embedded: Sample chạy engine + persistence + API + Studio (thay cho Workflow Server riêng trước đây).
// Standalone: Sample là client, gọi Workflow Server tại Elsa:ServerUrl.
var workflowEnabled = builder.Configuration.GetValue("Elsa:Enabled", true);
var workflowOptions = ElsaWorkflowOptions.FromConfiguration(builder.Configuration);
if (workflowEnabled)
{
    var redisConfiguration = builder.Configuration["Cache:DistributedGroups:Redis:Default:Configuration"]
        ?? throw new InvalidOperationException("Cache:DistributedGroups:Redis:Default:Configuration is not configured.");

    builder.Services.AddPlatformWorkflowServices(
        redisConfiguration,
        options => options.ConfigureFromSection(builder.Configuration));

    var workflowModule = builder.AddElsaWorkflowModule();
    if (workflowOptions.Mode == WorkflowExecutionMode.Embedded)
    {
        workflowModule
            .UseEntityFramework()
            .UseHttpApi()
            .UseStudio()
            .AddWorkflow<OnboardingProcessWorkflow>()
            .AddWorkflow<HrHandoverDeviceWorkflow>()
            .AddWorkflow<ItCreateAdWorkflow>()
            .AddWorkflow<ItCreateJiraWorkflow>()
            .AddWorkflow<ItCreateConfluenceWorkflow>()
            .AddWorkflow<ItCreateBitbucketWorkflow>()
            .AddWorkflow<HrSignProbationContractWorkflow>()
            .AddWorkflow<EmployeeConfirmOnboardWorkflow>()
            .ApplyEmbedded();
    }
    else
    {
        workflowModule.ApplyStandaloneClient();
    }
}

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

if (workflowEnabled)
{
    if (workflowOptions.Mode == WorkflowExecutionMode.Embedded)
    {
        app.UseWorkflowApi();
        app.UseWorkflowStudio();
    }
    else
    {
        var workflowStudioUrl = workflowOptions.GetResolvedStudioUrl()
            ?? throw new InvalidOperationException(
                "Elsa Workflow Studio URL is not configured. Configure 'Elsa:StudioUrl' or 'Elsa:ServerUrl'.");

        app.MapGet("/preview/{definitionId}/{instanceId?}",
            (string definitionId, string? instanceId) =>
            {
                var previewPath = $"/preview/{Uri.EscapeDataString(definitionId)}";
                if (!string.IsNullOrWhiteSpace(instanceId))
                    previewPath += $"/{Uri.EscapeDataString(instanceId)}";

                return Results.Redirect($"{workflowStudioUrl}{previewPath}");
            });
    }
}

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

if (workflowEnabled && workflowOptions.Mode == WorkflowExecutionMode.Embedded)
{
    var elsaConnectionString = app.Configuration.GetConnectionString(workflowOptions.ConnectionStringName);
    if (string.IsNullOrWhiteSpace(elsaConnectionString))
    {
        throw new InvalidOperationException(
            $"Connection string '{workflowOptions.ConnectionStringName}' is not configured or empty.");
    }

    app.EnsureMigrateTemplateDb<ManagementElsaDbContext>((_, options) => options.UseNpgsql(elsaConnectionString));
    app.EnsureMigrateTemplateDb<RuntimeElsaDbContext>((_, options) => options.UseNpgsql(elsaConnectionString));
}

await app.SeedSampleAuthorizationAsync();
app.Run();