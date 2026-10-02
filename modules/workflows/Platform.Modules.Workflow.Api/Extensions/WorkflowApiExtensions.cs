using Elsa.Extensions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerGen;
using Platform.Modules.ElsaWorkflow.Extensions;
using Platform.Workflow.Configuration;

namespace Platform.Modules.ElsaWorkflow.Api.Extensions;

public static class WorkflowApiExtensions
{
    public static ElsaWorkflowModuleBuilder UseHttpApi(this ElsaWorkflowModuleBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ConfigureElsa(elsa => elsa.UseWorkflowsApi());
        builder.HostBuilder.Services.PostConfigure<SwaggerGenOptions>(options =>
        {
            options.CustomSchemaIds(GetSchemaId);
        });

        return builder;
    }

    public static IApplicationBuilder UseWorkflowApi(this IApplicationBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        var options = app.ApplicationServices.GetService<ElsaWorkflowOptions>()
            ?? ElsaWorkflowOptions.FromConfiguration(app.ApplicationServices.GetRequiredService<IConfiguration>());

        var apiPrefix = options.ApiPathPrefix.TrimEnd('/');

        var authorizationPolicy = options.ApiAuthorizationPolicy ?? options.AuthorizationPolicy;
        if (!string.IsNullOrWhiteSpace(authorizationPolicy))
        {
            app.UseWhen(
                context => context.Request.Path.StartsWithSegments(apiPrefix, StringComparison.OrdinalIgnoreCase),
                branch => branch.Use(async (context, next) =>
                {
                    var authorization = context.RequestServices.GetRequiredService<IAuthorizationService>();
                    var result = await authorization.AuthorizeAsync(
                        context.User,
                        resource: null,
                        policyName: authorizationPolicy);

                    if (result.Succeeded)
                    {
                        await next();
                        return;
                    }

                    if (context.User.Identity?.IsAuthenticated == true)
                        await context.ForbidAsync();
                    else
                        await context.ChallengeAsync();
                }));
        }

        app.UseWorkflowsApi();

        return app;
    }

    private static string GetSchemaId(Type type)
    {
        if (type.FullName?.StartsWith("Elsa.", StringComparison.Ordinal) == true)
        {
            return type.FullName
                .Replace(".", "_", StringComparison.Ordinal)
                .Replace("+", "_", StringComparison.Ordinal)
                .Replace("`", "_", StringComparison.Ordinal)
                .Replace("[", "_", StringComparison.Ordinal)
                .Replace("]", string.Empty, StringComparison.Ordinal)
                .Replace(", ", "_", StringComparison.Ordinal);
        }

        if (!type.IsConstructedGenericType)
            return type.Name.Replace("[]", "Array", StringComparison.Ordinal);

        var prefix = string.Concat(type.GetGenericArguments().Select(GetSchemaId));
        return prefix + type.Name.Split('`')[0];
    }
}
