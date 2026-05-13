using Microsoft.AspNetCore.Http;

namespace Jarvis.Domain.DataStorages;

/// <summary>
/// Uses the HTTP request host (hostname, without port) for tenant identification.
/// </summary>
public class HostTenantIdResolver(IHttpContextAccessor httpContextAccessor) : ITenantIdResolver
{
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

    public string? GetTenantId()
    {
        var host = _httpContextAccessor.HttpContext?.Request.Host.Host;
        return string.IsNullOrWhiteSpace(host) ? null : host;
    }

    public Task<string?> GetTenantIdAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(GetTenantId());
}
