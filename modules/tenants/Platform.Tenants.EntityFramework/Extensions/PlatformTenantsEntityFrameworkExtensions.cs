using Microsoft.Extensions.DependencyInjection.Extensions;
using Platform.DDD.Domain.Repositories;
using Platform.Tenants.EntityFramework.Persistence;
using Platform.Tenants.Extensions;
using Platform.Tenants.Persistence;

namespace Platform.Tenants.EntityFramework.Extensions;

/// <summary>
/// Đăng ký store catalog trên Unit of Work Master. Không gọi <c>SwitchDbContextAsync</c>:
/// bản ghi tenant nằm ở Master, chuỗi kết nối của từng tenant do Multitenancy resolve sau.
/// </summary>
public static class PlatformTenantsEntityFrameworkExtensions
{
    public static PlatformTenantsBuilder<TTenant> UseEntityFramework<TTenant, TUnitOfWork>(
        this PlatformTenantsBuilder<TTenant> builder)
        where TTenant : class, ITenantCatalogEntity
        where TUnitOfWork : class, IUnitOfWork
    {
        ArgumentNullException.ThrowIfNull(builder);
        builder.HostBuilder.Services.TryAddScoped<ITenantStore<TTenant>, TenantStore<TTenant, TUnitOfWork>>();
        return builder;
    }
}
