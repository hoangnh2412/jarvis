// Program.cs — single-project bootstrap (legacy / Sample-style)
// Solution phân lớp: dùng templates/layers/Program.cs + HostLayerExtension thay file này.
// Replace {App} with your application namespace.

using Jarvis.Domain;
using Jarvis.EntityFramework;
using Jarvis.HealthChecks;
using Jarvis.Mvc;
using Jarvis.Mvc.ApplicationBuilders;
using Jarvis.Mvc.ExceptionHandling;
using Jarvis.OpenTelemetry.Abstractions;
using Jarvis.OpenTelemetry.Extensions;
using Jarvis.Swashbuckle;
using {App}.Persistence;
using {App}.Services;

var builder = WebApplication.CreateBuilder(args);

// --- OpenTelemetry (optional — see telemetry-dotnet skill) ---
builder.Services
    .AddJarvisOpenTelemetry(builder.Configuration, services =>
    {
        services.AddScoped<IEnrichLogService, EnrichLogService>();
        services.AddScoped<IEnrichTraceService, EnrichTraceService>();
    })
    .ConfigureResource()
    .ConfigureLogging()
    .ConfigureTrace()
    .ConfigureMetric();

// --- Foundation ---
builder.AddCoreJson();
builder.AddCoreCors();
builder.AddCoreDomain();
builder.AddCoreWebApi();

// --- EF Core ---
builder.AddEntityFramework();
builder.Add{App}DbContext(); // host extension: AddCoreDbContext<,,>

// --- Swagger ---
builder.AddCoreSwagger();

// --- Health checks (optional — see healthcheck-dotnet skill) ---
builder.AddHealthChecks();
// builder.Add{App}ReadinessHealthChecks();

var app = builder.Build();

app.UseCoreSwagger();
app.UseHttpsRedirection();
app.UseCoreCors();
app.UseJarvisOpenTelemetry();
app.UseCoreMiddleware<ApiResponseWrapperMiddleware>();
app.MapControllers();
app.UseHealthChecks();

app.Run();
