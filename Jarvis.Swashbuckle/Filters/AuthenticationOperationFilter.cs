using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Jarvis.Swashbuckle.Filters;

public class AuthenticationOperationFilter : IOperationFilter
{
    private readonly string[] _types;

    public AuthenticationOperationFilter(string types)
    {
        _types = types.Split('|');
    }

    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var controllerAttrs = context.MethodInfo.ReflectedType?.GetCustomAttributes(true) ?? Array.Empty<object>();
        var actionAttrs = context.MethodInfo.GetCustomAttributes(true);

        if (actionAttrs.OfType<AllowAnonymousAttribute>().Any())
            return;

        var actionAuthorize = actionAttrs.OfType<AuthorizeAttribute>().ToList();
        List<AuthorizeAttribute> authorizeSources;
        if (actionAuthorize.Count > 0)
        {
            authorizeSources = actionAuthorize;
        }
        else
        {
            if (controllerAttrs.OfType<AllowAnonymousAttribute>().Any())
                return;

            authorizeSources = controllerAttrs.OfType<AuthorizeAttribute>().ToList();
        }

        if (authorizeSources.Count == 0)
            return;

        var scopes = authorizeSources
            .Select(a => a.Policy)
            .Where(p => !string.IsNullOrEmpty(p))
            .Distinct()
            .ToList();

        var requirements = new List<OpenApiSecurityRequirement>();
        foreach (var item in _types)
        {
            var scheme = new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = item
                }
            };
            requirements.Add(new OpenApiSecurityRequirement
            {
                [scheme] = scopes
            });
        }

        operation.Security = requirements;
    }
}
