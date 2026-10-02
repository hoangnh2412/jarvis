using Elsa.Common.Multitenancy;
using Platform.DDD.Domain.Services;

namespace Platform.Workflow.Multitenancy;

/// <summary>
/// Tenant provider cho Elsa Workflow, tự động chấp nhận tenant hiện tại từ Platform context
/// và default tenant để đảm bảo không bị chặn bởi whitelist tĩnh khi chạy đa tenant.
/// </summary>
public sealed class PlatformTenantsProvider(ICurrentTenantAccessor currentTenantAccessor) : ITenantsProvider
{
    public Task<IEnumerable<Tenant>> ListAsync(CancellationToken cancellationToken = default)
    {
        var tenants = new List<Tenant> { Tenant.Default };

        var currentTenantId = currentTenantAccessor.TenantId?.ToString("D");
        if (currentTenantId != null)
        {
            tenants.Add(new Tenant
            {
                Id = currentTenantId,
                Name = currentTenantId
            });
        }

        return Task.FromResult<IEnumerable<Tenant>>(tenants);
    }

    public Task<Tenant?> FindAsync(TenantFilter filter, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(filter);

        if (string.Equals(filter.Id, Tenant.DefaultTenantId, StringComparison.OrdinalIgnoreCase))
        {
            return Task.FromResult<Tenant?>(Tenant.Default);
        }

        if (string.Equals(filter.Id, Tenant.AgnosticTenantId, StringComparison.OrdinalIgnoreCase))
        {
            return Task.FromResult<Tenant?>(new Tenant
            {
                Id = Tenant.AgnosticTenantId,
                Name = "Tenant Agnostic"
            });
        }

        if (!string.IsNullOrWhiteSpace(filter.Id))
        {
            return Task.FromResult<Tenant?>(new Tenant
            {
                Id = filter.Id,
                Name = filter.Id
            });
        }

        return Task.FromResult<Tenant?>(null);
    }
}
