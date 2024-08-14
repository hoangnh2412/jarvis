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
        var controllerAttrs = context.MethodInfo.ReflectedType?.GetCustomAttributes(true);
        var actionAttrs = context.MethodInfo.GetCustomAttributes(true);

        var scopes = new List<string>();

        if (controllerAttrs != null && controllerAttrs.Length > 0)
        {
#pragma warning disable CS8620 // Argument cannot be used for parameter due to differences in the nullability of reference types.
            scopes.AddRange(controllerAttrs.OfType<AuthorizeAttribute>().Select(attr => attr.Policy).Distinct());
#pragma warning restore CS8620 // Argument cannot be used for parameter due to differences in the nullability of reference types.
        }

        if (actionAttrs != null && actionAttrs.Length > 0)
#pragma warning disable CS8620 // Argument cannot be used for parameter due to differences in the nullability of reference types.
            scopes.AddRange(actionAttrs.OfType<AuthorizeAttribute>().Select(attr => attr.Policy).Distinct());
#pragma warning restore CS8620 // Argument cannot be used for parameter due to differences in the nullability of reference types.

        if (scopes.Count == 0)
            return;

#pragma warning disable CS8604 // Possible null reference argument.
        var allows = actionAttrs.OfType<AllowAnonymousAttribute>().Distinct().ToList();
#pragma warning restore CS8604 // Possible null reference argument.
        if (allows.Count > 0)
            return;

#pragma warning disable CS8604 // Possible null reference argument.
        allows = controllerAttrs.OfType<AllowAnonymousAttribute>().Distinct().ToList();
#pragma warning restore CS8604 // Possible null reference argument.
        if (allows.Count > 0)
            return;

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
            requirements.Add(new OpenApiSecurityRequirement()
            {
                [scheme] = scopes
            });
        }

        operation.Security = requirements;
    }
}