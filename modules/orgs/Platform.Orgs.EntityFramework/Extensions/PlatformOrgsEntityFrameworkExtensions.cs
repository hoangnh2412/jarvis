using Microsoft.Extensions.DependencyInjection.Extensions;
using Platform.DDD.Domain.Repositories;
using Platform.DDD.Domain.Services;
using Platform.Orgs;
using Platform.Orgs.EntityFramework.Persistence;
using Platform.Orgs.Extensions;
using Platform.Orgs.Persistence;

namespace Platform.Orgs.EntityFramework.Extensions;

/// <summary>Đăng ký <see cref="OrgStore{TOrg,TUnitOfWork}"/>. Host vẫn gọi <c>ApplyPlatformOrgCatalog</c> trong <c>OnModelCreating</c>.</summary>
public static class PlatformOrgsEntityFrameworkExtensions
{
    public static PlatformOrgsBuilder<TOrg, TTenant> UseEntityFramework<TOrg, TTenant, TUnitOfWork>(
        this PlatformOrgsBuilder<TOrg, TTenant> builder)
        where TOrg : class, IOrgCatalogEntity
        where TTenant : class, ICurrentTenantIdentity
        where TUnitOfWork : class, IUnitOfWork
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.HostBuilder.Services.TryAddScoped<IOrgStore<TOrg>, OrgStore<TOrg, TUnitOfWork>>();
        return builder;
    }
}
