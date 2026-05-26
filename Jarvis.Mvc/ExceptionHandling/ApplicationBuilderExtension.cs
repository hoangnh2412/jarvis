using Jarvis.Mvc.Attributes;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Text.RegularExpressions;

namespace Jarvis.Mvc.ExceptionHandling;

public static class ApplicationBuilderExtension
{
    /// <summary>
    /// Registers middleware <typeparamref name="T"/> with conditional execution from configuration.
    /// Middleware runs only when:
    /// - <c>IsEnable</c> is true.
    /// - Request path matches at least one regex in <c>Includes</c> (when Includes is not empty).
    /// - Request path does not match any regex in <c>Excludes</c>.
    /// - Endpoint is not marked with <see cref="IgnoreMiddlewareAttribute"/> for this middleware type.
    /// Configuration section lookup order:
    /// <c>Middlewares:{name}Middleware</c> then <c>Middlewares:{name}</c>.
    /// </summary>
    /// <typeparam name="T">Middleware type to register.</typeparam>
    /// <param name="app">Application builder.</param>
    /// <returns>The same application builder for chaining.</returns>
    public static IApplicationBuilder UseCoreMiddleware<T>(this IApplicationBuilder app)
    {
        var name = typeof(T).Name.Replace("Middleware", "");

        var configuration = app.ApplicationServices.GetRequiredService<IConfiguration>();
        var option = configuration.GetSection($"Middlewares:{name}Middleware").Get<MiddlewareOption>();
        if (option == null)
            option = configuration.GetSection($"Middlewares:{name}").Get<MiddlewareOption>();

        if (option == null)
            return app.UseMiddleware<T>();

        if (!option.IsEnable)
            return app;

        app.UseWhen(httpContext =>
        {
            var requestPath = httpContext.Request.Path.Value ?? string.Empty;

            if (option.Includes.Length > 0)
            {
                var isIncluded = false;
                foreach (var pattern in option.Includes)
                {
                    if (string.IsNullOrWhiteSpace(pattern))
                        continue;

                    if (Regex.IsMatch(requestPath, pattern, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
                    {
                        isIncluded = true;
                        break;
                    }
                }

                if (!isIncluded)
                    return false;
            }

            foreach (var pattern in option.Excludes)
            {
                if (string.IsNullOrWhiteSpace(pattern))
                    continue;

                if (Regex.IsMatch(requestPath, pattern, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
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