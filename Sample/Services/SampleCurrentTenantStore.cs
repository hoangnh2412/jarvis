using Jarvis.DDD.Domain.Services;
using Jarvis.Multitenancy;

namespace Sample.Services;

/// <summary>
/// Store demo cho Sample: map tenant id → <see cref="CurrentTenantInfo"/>.
/// Host thật thay bằng DB/cache.
/// </summary>
public sealed class SampleCurrentTenantStore : ICurrentTenantStore<CurrentTenantInfo>
{
    public Task<CurrentTenantInfo?> FindAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        return Task.FromResult<CurrentTenantInfo?>(new CurrentTenantInfo
        {
            TenantId = tenantId,
            Name = $"tenant-{tenantId:N}",
        });
    }
}
