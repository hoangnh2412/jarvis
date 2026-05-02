using Sample;
using Jarvis.OpenTelemetry;
using Sample.Persistence;
using Jarvis.EntityFramework;
using Jarvis.Mvc;
using Jarvis.Mvc.ExceptionHandling;
using Jarvis.Swashbuckle;
using Asp.Versioning;
using Jarvis.OpenTelemetry.Interfaces;
using Jarvis.Domain.Services;
using Jarvis.Domain;
using Jarvis.Mvc.ApplicationBuilders;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddTelemetry(builder.Configuration, (services) =>
    {
        services.AddScoped<IEnrichLogService, EnrichLogService>();
        services.AddScoped<IEnrichTraceService, EnrichTraceService>();
    })
    .ConfigureResource()
    .ConfigureLogging()
    .ConfigureTrace()
    .ConfigureMetric();

builder.AddCoreSwagger();
builder.AddCoreJson();
builder.AddCoreCors();
builder.AddCoreDomain();

builder.AddCoreWebApi();
builder.AddEntityFramework();
builder.AddSampleDbContext();

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

var app = builder.Build();
app.UseCoreSwagger();
app.UseHttpsRedirection();

app.UseCoreCors();

app.UseOTEL();
app.UseCoreMiddleware<ApiResponseWrapperMiddleware>();
app.MapControllers();

app.EnsureMigrateDb<ISampleUnitOfWork>();
app.Run();