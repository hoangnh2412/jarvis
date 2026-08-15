using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Jarvis.Swashbuckle;

public static class AppBuilderExtension
{
    public static IApplicationBuilder UseCoreSwagger(this IApplicationBuilder app)
    {
        var swaggerOption = app.ApplicationServices.GetRequiredService<IOptions<SwaggerOption>>().Value;
        if (!swaggerOption.Enable)
            return app;

        var prefix = swaggerOption.Prefix?.Trim().Trim('/') ?? string.Empty;
        var swaggerRoot = string.IsNullOrEmpty(prefix)
            ? "swagger"
            : $"{prefix}/swagger";

        app.UseSwagger(options =>
        {
            options.RouteTemplate = $"{swaggerRoot}/{{documentName}}/swagger.json";
        });

        app.UseSwaggerUI(options =>
        {
            if (swaggerOption.Versions == null || swaggerOption.Versions.Length == 0)
                return;

            options.RoutePrefix = swaggerRoot;

            var appName = Assembly.GetEntryAssembly()?.GetName().Name ?? "API";
            foreach (var version in swaggerOption.Versions)
            {
                var jsonPath = $"/{swaggerRoot}/{version}/swagger.json";
                options.SwaggerEndpoint(jsonPath, $"{appName} {version}");
            }
        });
        return app;
    }
}
