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

        app.UseSwagger(options =>
        {
            options.RouteTemplate = swaggerOption.Prefix + "/swagger/{documentName}/swagger.json";
        });

        app.UseSwaggerUI(options =>
        {
            if (swaggerOption.Versions == null || swaggerOption.Versions.Length == 0)
                return;

            foreach (var version in swaggerOption.Versions)
            {
                if (string.IsNullOrEmpty(swaggerOption.Prefix))
                {
                    options.SwaggerEndpoint($"{swaggerOption.Prefix}/swagger/{version}/swagger.json", $"{Assembly.GetEntryAssembly()?.GetName().Name} API {version}");
                }
                else
                {
                    options.SwaggerEndpoint($"/{swaggerOption.Prefix}/swagger/{version}/swagger.json", $"{Assembly.GetEntryAssembly()?.GetName().Name} API {version}");
                    options.RoutePrefix = $"{swaggerOption.Prefix}/swagger";
                }
            }
        });
        return app;
    }
}