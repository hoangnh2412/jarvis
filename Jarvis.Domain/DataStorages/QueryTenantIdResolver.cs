using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace Jarvis.Domain.DataStorages;

/// <summary>
/// Use query string of request to tenant identification
/// </summary>
public class QueryTenantIdResolver(
    IHttpContextAccessor httpContextAccessor,
    IConfiguration configuration)
    : ITenantIdResolver
{
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
    private readonly string _tenantQueryName = configuration.GetValue<string>("TenantQueryName") ?? "tenantId";

    public Task<Guid?> GetTenantIdAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (_httpContextAccessor.HttpContext == null)
            return Task.FromResult<Guid?>(null);

        var raw = _httpContextAccessor.HttpContext.Request.Query[_tenantQueryName].ToString();
        return Task.FromResult(TenantIdGuidParser.Parse(raw));
    }
}
