using Microsoft.AspNetCore.Http;

namespace Jarvis.Domain.DataStorages;

/// <summary>
/// Use header of request to tenant identification
/// </summary>
public class HeaderTenantIdResolver(
    IHttpContextAccessor httpContextAccessor,
    string headerName = "X-Tenant-Id")
    : ITenantIdResolver
{
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

    public string? GetTenantId() => _httpContextAccessor.HttpContext?.Request.Headers[headerName].ToString() ?? null;

    public Task<string?> GetTenantIdAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(GetTenantId());
    }
}