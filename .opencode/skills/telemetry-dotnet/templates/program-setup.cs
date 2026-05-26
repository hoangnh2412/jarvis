// Program.cs — minimal Jarvis OpenTelemetry wiring
// Replace {App} with your application namespace.

using Jarvis.OpenTelemetry.Abstractions;
using Jarvis.OpenTelemetry.Extensions;
using {App}.Services; // EnrichTraceService, EnrichLogService

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddJarvisOpenTelemetry(builder.Configuration, services =>
    {
        services.AddScoped<IEnrichLogService, EnrichLogService>();
        services.AddScoped<IEnrichTraceService, EnrichTraceService>();
        // Plug-in: services.AddSingleton<ITraceInstrumentation, MyTraceInstrumentation>();
        // Redis Jarvis package: services.AddJarvisRedisTraceInstrumentation();
    })
    .ConfigureResource()
    .ConfigureLogging()
    .ConfigureTrace(options =>
    {
        // providers/entityframework, providers/redis — thêm instrumentation tại đây
    })
    .ConfigureMetric();

var app = builder.Build();

app.UseHttpsRedirection();
app.MapControllers();

// Sau routing nếu enrich phụ thuộc endpoint
app.UseJarvisOpenTelemetry();

app.Run();
