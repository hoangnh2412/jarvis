using Jarvis.Domain.Entities;
using Jarvis.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Jarvis.EntityFramework.DataStorages;

internal static class TenantScopedContextValidation
{
    private static readonly AsyncLocal<int> _suppressDepth = new();

    internal const string TenantRequiredMessage =
        "TenantId is required for this DbContext because it maps tenant-scoped entities (ITenantEntity). " +
        "Provide a tenant (e.g. X-Tenant-Id header), call SwitchDbContextAsync(tenantId), or use IgnoreQueryFilters() for cross-tenant reads.";

    internal static bool IsSuppressed => _suppressDepth.Value > 0;

    internal static IDisposable BeginSuppressScope()
    {
        _suppressDepth.Value++;
        return new SuppressScope();
    }

    internal static bool HasTenantFilteredEntities(DbContext context) =>
        context.Model.GetEntityTypes()
            .Any(et => typeof(ITenantEntity).IsAssignableFrom(et.ClrType));

    internal static void EnsureTenantIdForSave(DbContext context)
    {
        if (IsSuppressed || !RequiresTenant(context))
            return;

        throw new InvalidOperationException(TenantRequiredMessage);
    }

    private static bool RequiresTenant(DbContext context)
    {
        if (!HasTenantFilteredEntities(context))
            return false;

        if (context is IStorageContext storage && storage.TenantId.HasValue)
            return false;

        return true;
    }

    private sealed class SuppressScope : IDisposable
    {
        public void Dispose()
        {
            if (_suppressDepth.Value > 0)
                _suppressDepth.Value--;
        }
    }
}
