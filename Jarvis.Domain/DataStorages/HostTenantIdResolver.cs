using Microsoft.AspNetCore.Http;

namespace Jarvis.Domain.DataStorages;

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
