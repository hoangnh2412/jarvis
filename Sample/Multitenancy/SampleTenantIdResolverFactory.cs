using Jarvis.Domain.DataStorages;

namespace Sample.Multitenancy;

/// <summary>
/// Resolves tenant from <see cref="JobTenantContext"/> first (background jobs), then HTTP resolvers (header, user, query, host).
/// </summary>
public sealed class SampleTenantIdResolverFactory(
    IServiceProvider serviceProvider,
    JobTenantContext jobTenantContext)
    : ITenantIdResolverFactory
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;
    private readonly JobTenantContext _jobTenantContext = jobTenantContext;

    public async Task<Guid?> GetTenantIdAsync(CancellationToken cancellationToken = default)
    {
        if (_jobTenantContext.TenantId.HasValue)
            return _jobTenantContext.TenantId;

        return await new TenantIdResolverFactory(_serviceProvider)
            .GetTenantIdAsync(cancellationToken)
            .ConfigureAwait(false);
    }
}
