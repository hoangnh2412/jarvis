using Microsoft.AspNetCore.Authentication;
using Elsa.Studio.Authentication.Abstractions.Contracts;
using Microsoft.AspNetCore.Http.Connections.Client;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Platform.DDD.Domain.Services;

namespace Platform.Modules.ElsaWorkflow.Api.Authentication;

public sealed class WorkflowStudioHttpConnectionOptionsConfigurator(
    IHttpContextAccessor httpContextAccessor,
    ICurrentTenantAccessor currentTenantAccessor) : IHttpConnectionOptionsConfigurator
{
    public Task ConfigureAsync(
        HttpConnectionOptions options,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(options);

        var httpContext = httpContextAccessor.HttpContext;
        if (httpContext is not null)
        {
            var configuration = httpContext.RequestServices.GetService<IConfiguration>();
            var headerKey = configuration?.GetValue<string>("TenantHeaderKey") ?? "X-Tenant-Id";
            var tenantId = currentTenantAccessor.TenantId;
            if (tenantId.HasValue)
            {
                options.Headers ??= new Dictionary<string, string>();
                options.Headers[headerKey] = tenantId.Value.ToString("D");
            }
        }

        options.AccessTokenProvider = async () =>
        {
            var ctx = httpContextAccessor.HttpContext;
            if (ctx is null)
                return null;

            var token = await ctx.GetTokenAsync("access_token");
            if (!string.IsNullOrWhiteSpace(token))
                return token;

            var authorization = ctx.Request.Headers.Authorization.ToString();
            return authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
                ? authorization["Bearer ".Length..].Trim()
                : null;
        };

        return Task.CompletedTask;
    }
}
