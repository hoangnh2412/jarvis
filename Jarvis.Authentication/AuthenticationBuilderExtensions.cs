using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;

namespace Jarvis.Authentication;

public static class AuthenticationBuilderExtensions
{
    public static AuthenticationBuilder AddJarvisCompositeScheme(
        this AuthenticationBuilder builder,
        string apiKeyHeaderName = "X-API-KEY",
        bool includeBasic = false)
    {
        return builder.AddPolicyScheme(
            JarvisAuthenticationSchemes.Composite,
            "Jarvis authentication",
            options =>
            {
                options.ForwardDefaultSelector = context =>
                {
                    if (context.Request.Headers.ContainsKey(apiKeyHeaderName))
                        return JarvisAuthenticationSchemes.ApiKey;

                    var authorization = context.Request.Headers.Authorization.ToString();
                    if (includeBasic && authorization.StartsWith("Basic ", StringComparison.OrdinalIgnoreCase))
                        return JarvisAuthenticationSchemes.Basic;

                    if (authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                        return "Bearer";

                    return includeBasic
                        ? JarvisAuthenticationSchemes.Basic
                        : "Bearer";
                };
            });
    }
}
