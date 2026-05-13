using Jarvis.Domain.DataStorages;
using Jarvis.Domain.Repositories;

namespace Jarvis.EntityFramework.DataStorages;

internal sealed class StorageContextTenantInitializer(ITenantIdResolver tenantIdResolver)
    : IStorageContextTenantInitializer
{
    public void Initialize(IStorageContext context)
    {
        var id = tenantIdResolver.GetTenantId();
        if (id != null)
            context.SetTenantId(id);
    }

    public async ValueTask InitializeAsync(IStorageContext context, CancellationToken cancellationToken = default)
    {
        var id = await tenantIdResolver.GetTenantIdAsync(cancellationToken).ConfigureAwait(false);
        if (id != null)
            context.SetTenantId(id);
    }
}
