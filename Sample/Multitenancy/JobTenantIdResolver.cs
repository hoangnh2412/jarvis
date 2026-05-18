using Jarvis.Domain.DataStorages;

namespace Sample.Multitenancy;

public sealed class JobTenantIdResolver(JobTenantContext jobTenantContext) : ITenantIdResolver
{
    private readonly JobTenantContext _jobTenantContext = jobTenantContext;

    public Task<Guid?> GetTenantIdAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(_jobTenantContext.TenantId);
}
