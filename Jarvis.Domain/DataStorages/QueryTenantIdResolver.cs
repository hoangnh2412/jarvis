using Microsoft.AspNetCore.Http;

namespace Jarvis.Domain.DataStorages;

/// <summary>
/// Use query string of request to tenant identification
/// </summary>
public class QueryTenantIdResolver(
    IHttpContextAccessor httpContextAccessor,
    string paramName = "tenantId")
    : ITenantIdResolver
{
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

    public string? GetTenantId() => _httpContextAccessor.HttpContext?.Request.Query[paramName].ToString() ?? null;

    public Task<string?> GetTenantIdAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(GetTenantId());
    }
}