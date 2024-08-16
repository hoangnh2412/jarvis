using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Jarvis.Mvc.ExceptionHandling;

public static class ApplicationBuilderExtension
{
    public static IApplicationBuilder UseCoreMiddleware<T>(this IApplicationBuilder app)
    {
        var name = typeof(T).Name.Replace("Middleware", "");

        var configuration = app.ApplicationServices.GetRequiredService<IConfiguration>();
        var option = configuration.GetSection($"Middlewares:{name}Middleware").Get<MiddlewareOption>();
        if (option == null)
            option = configuration.GetSection($"Middlewares:{name}").Get<MiddlewareOption>();

        if (option == null)
            return app;

        if (!option.IsEnable)
            return app;

        var pathStartWiths = option.Ignores["PathStartWith"];
        var paths = option.Ignores["Path"];
        var controllerActions = option.Ignores["ControllerAction"];

        app.UseWhen(httpContext =>
        {
            if (paths.Contains(httpContext.Request.Path.Value))
                return false;

            foreach (var item in pathStartWiths)
            {
                if (httpContext.Request.Path.StartsWithSegments(item))
                    return false;
            }

            var endpoint = httpContext.GetEndpoint();
            if (endpoint == null)
                return true;

            var controllerActionDescriptor = endpoint.Metadata.GetMetadata<ControllerActionDescriptor>();
            if (controllerActionDescriptor == null)
                return true;

            if (controllerActions.Contains($"{controllerActionDescriptor.ControllerName}.{controllerActionDescriptor.ActionName}"))
                return false;

            return true;
        }, x => x.UseMiddleware<T>());

        return app;
    }
}