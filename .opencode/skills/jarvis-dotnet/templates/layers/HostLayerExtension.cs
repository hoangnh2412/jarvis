using {Product}.Application.DependencyInjection;
using {Product}.Infrastructure.DependencyInjection;
using {Product}.Host.Services;
using Asp.Versioning;
using Jarvis.HealthChecks;
using Jarvis.Mvc;
using Jarvis.Mvc.ApplicationBuilders;
using Jarvis.Mvc.ExceptionHandling;
using Jarvis.OpenTelemetry.Abstractions;
using Jarvis.OpenTelemetry.Extensions;
using Jarvis.Swashbuckle;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace {Product}.Host.DependencyInjection;

public static class HostLayerExtension
{
  public static IHostApplicationBuilder AddHostLayer(this IHostApplicationBuilder builder)
  {
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

    builder.AddApplicationLayer();
    builder.AddInfrastructureLayer();

    builder.AddCoreJson();
    builder.AddCoreCors();
    builder.AddCoreDomain();
    builder.AddCoreWebApi();

    builder.Services.AddApiVersioning(options =>
    {
      options.DefaultApiVersion = new ApiVersion(1, 0);
      options.AssumeDefaultVersionWhenUnspecified = true;
      options.ReportApiVersions = true;
    }).AddApiExplorer(options =>
    {
      options.GroupNameFormat = "'v'VVV";
      options.SubstituteApiVersionInUrl = true;
    });

    builder.AddCoreSwagger();
    builder.AddHealthChecks();

    return builder;
  }

  public static WebApplication UseHostLayer(this WebApplication app)
  {
    app.UseCoreSwagger();
    app.UseHttpsRedirection();
    app.UseCoreCors();
    app.UseJarvisOpenTelemetry();
    app.UseCoreMiddleware<ApiResponseWrapperMiddleware>();
    app.MapControllers();
    app.UseHealthChecks();
    return app;
  }
}
