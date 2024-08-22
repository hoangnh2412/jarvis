using Jarvis.Mvc.Attributes;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
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

        var pathStartWith = option.Ignores["PathStartWith"];
        var path = option.Ignores["Path"];

        app.UseWhen(httpContext =>
        {
            if (path.Contains(httpContext.Request.Path.Value))
                return false;

            foreach (var item in pathStartWith)
            {
                if (httpContext.Request.Path.StartsWithSegments(item))
                    return false;
            }

            var endpoint = httpContext.GetEndpoint();
            if (endpoint == null)
                return true;

            var attributes = endpoint.Metadata.OfType<IgnoreMiddlewareAttribute>();
            if (attributes.Any(x => x.Name == typeof(T).Name))
                return false;

            return true;
        }, x => x.UseMiddleware<T>());

        return app;
    }
}