using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace Jarvis.Domain.DataStorages;

/// <summary>
/// Use header of request to tenant identification
/// </summary>
public class HeaderTenantIdResolver(
    IHttpContextAccessor httpContextAccessor,
    IConfiguration configuration)
    : ITenantIdResolver
{
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
    private readonly string _tenantHeaderKey = configuration.GetValue<string>("TenantHeaderKey") ?? "X-Tenant-Id";

    public Task<Guid?> GetTenantIdAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (_httpContextAccessor.HttpContext == null)
            return Task.FromResult<Guid?>(null);

        var raw = _httpContextAccessor.HttpContext.Request.Headers[_tenantHeaderKey].ToString();
        return Task.FromResult(TenantIdGuidParser.Parse(raw));
    }
}
