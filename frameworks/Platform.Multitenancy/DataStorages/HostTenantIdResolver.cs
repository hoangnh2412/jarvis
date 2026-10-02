using Microsoft.AspNetCore.Http;
using Platform.DDD.Domain.DataStorages;

namespace Platform.Multitenancy.DataStorages;

/// <summary>
/// Uses the HTTP request host when it is a valid <see cref="Guid"/> string.
/// </summary>
public class HostTenantIdResolver(IHttpContextAccessor httpContextAccessor) : ITenantIdResolver
{
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

    public Task<Guid?> GetTenantIdAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var host = _httpContextAccessor.HttpContext?.Request.Host.Host;
        return Task.FromResult(TenantIdGuidParser.Parse(host));
    }
}
