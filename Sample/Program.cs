using Sample;
using Sample.Health;
using Sample.Persistence;
using Sample.Telemetry;
using Jarvis.EntityFramework;
using Jarvis.Mvc;
using Jarvis.Mvc.ExceptionHandling;
using Jarvis.Swashbuckle;
using Asp.Versioning;
using Jarvis.OpenTelemetry.Abstractions;
using Jarvis.OpenTelemetry.Extensions;
using Jarvis.Domain.Services;
using Jarvis.Domain;
using Jarvis.Mvc.ApplicationBuilders;
using Jarvis.HealthChecks;
using Serilog;
using StackExchange.Redis;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, _, configuration) => configuration.ReadFrom.Configuration(context.Configuration).Enrich.FromLogContext(), writeToProviders: true);
builder.Services
    .AddJarvisOpenTelemetry(builder.Configuration, services =>
    {
        services.AddScoped<IEnrichLogService, EnrichLogService>();
        services.AddScoped<IEnrichTraceService, EnrichTraceService>();
    })
    .ConfigureResource()
    .ConfigureLogging()
    .ConfigureTrace(options =>
    {
        options
            .AddEntityFrameworkCoreInstrumentation()
            .AddRedisInstrumentation("Default");
    })
    .ConfigureMetric();

builder.AddCoreSwagger();
builder.AddCoreJson();
builder.AddCoreCors();
builder.AddCoreDomain();

builder.AddCoreWebApi();
builder.AddEntityFramework();
builder.AddSampleDbContext();

// Redis (StackExchange) for Sample demos — same config shape as Cache:DistGroups:Redis:Default.
builder.Services.AddKeyedSingleton<IConnectionMultiplexer>("Default", (_, keyedService) =>
{
    var configuration = _.GetRequiredService<IConfiguration>();
    var redisConfig = configuration["Cache:DistGroups:Redis:Default:Configuration"];
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

// builder.Services.AddHostedService<Worker>();

builder.Services.AddHttpClient(SampleDogCeoApiHealthCheck.HttpClientName, client =>
{
    client.Timeout = TimeSpan.FromSeconds(5);
});
builder.Services.AddHttpClient(SampleArticArtworksApiHealthCheck.HttpClientName, client =>
{
    client.Timeout = TimeSpan.FromSeconds(5);
});
builder.AddHealthChecks();
builder.AddSampleReadinessHealthChecks();

var app = builder.Build();
app.UseCoreSwagger();
app.UseHttpsRedirection();

app.UseCoreCors();

// Demo headers for OTEL trace enrichment (request: send x-demo-request; response: x-demo-response).
app.UseMiddleware<SampleOtlpDemoHeadersMiddleware>();

// Custom metrics (OTLP via Jarvis ConfigureMetric → same meter name "Sample").
app.UseMiddleware<SampleApiCallMetricsMiddleware>();

app.UseSerilogRequestLogging();

app.UseJarvisOpenTelemetry();
app.UseCoreMiddleware<ApiResponseWrapperMiddleware>();
app.MapControllers();
app.UseHealthChecks();

app.EnsureMigrateDb<ISampleUnitOfWork>();
app.Run();