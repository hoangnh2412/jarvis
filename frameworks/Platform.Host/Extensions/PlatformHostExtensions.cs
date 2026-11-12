using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Platform.Authentication.Identity.Domain;
using Platform.Authorization.EntityFramework.Persistence;
using Platform.DDD.Application;
using Platform.DDD.Domain.Repositories;
using Platform.DDD.Domain.Services;
using Platform.Host.Entities;
using Platform.Host.Persistence;
using Platform.Modules.Account.Api.Extensions;
using Platform.Modules.Account.DependencyInjection;
using Platform.Modules.IdentityAdmin.Abstractions;
using Platform.Modules.IdentityAdmin.Api.Extensions;
using Platform.Modules.IdentityAdmin.Authorization;
using Platform.Modules.IdentityAdmin.DependencyInjection;
using Platform.Modules.IdentityAdmin.Services;
using Platform.Multitenancy;
using Platform.OrgStructure.Api.Extensions;
using Platform.OrgStructure.EntityFramework.Extensions;
using Platform.OrgStructure.EntityFramework.Persistence;
using Platform.OrgStructure.Extensions;
using Platform.Tenants.Services;

namespace Platform.Host.Extensions;

/// <summary>Đăng ký identity admin và org-structure với entity cùng DbContext mặc định.</summary>
public static class PlatformHostExtensions
{
    public static IHostApplicationBuilder AddPlatformHost<TTenantContext, TUnitOfWork, TIdentityContext>(
        this IHostApplicationBuilder builder)
        where TTenantContext : DbContext
        where TUnitOfWork : class, IUnitOfWork
        where TIdentityContext : IdentityDbContextBase
    {
        builder.AddPlatformIdentityAdmin<TIdentityContext>();
        builder.AddPlatformOrgStructure<TTenantContext, TUnitOfWork>();
        return builder;
    }

    public static IHostApplicationBuilder AddPlatformIdentityAdmin<TIdentityContext>(
        this IHostApplicationBuilder builder,
        params Assembly[] permissionAssemblies)
        where TIdentityContext : IdentityDbContextBase
    {
        builder.AddCoreApplication();
        builder.Services.AddIdentityAdminApplication();
        builder.Services.AddIdentityAdminApi();
        builder.Services
            .AddIdentityCore<User>(options => options.User.RequireUniqueEmail = false)
            .AddRoles<Role>()
            .AddEntityFrameworkStores<TIdentityContext>();

        var assemblies = new[] { typeof(SystemPermissions).Assembly }
            .Concat(permissionAssemblies)
            .Distinct()
            .ToArray();

        builder.Services.TryAddSingleton<IPermissionCatalogProvider>(_ =>
            new DefaultPermissionCatalogProvider(
                PermissionCatalogBuilder.Build(assemblies)));
        builder.Services.AddScoped<IAppIdentityStore>(serviceProvider =>
            new PlatformAppIdentityStore(serviceProvider.GetRequiredService<TIdentityContext>()));
        builder.Services.AddScoped<RealmUserOperations>();
        builder.Services.AddAccountApplication();
        builder.Services.AddAccountApi();

        return builder;
    }

    public static IHostApplicationBuilder AddPlatformOrgStructure<TTenantContext, TUnitOfWork>(
        this IHostApplicationBuilder builder)
        where TTenantContext : DbContext
        where TUnitOfWork : class, IUnitOfWork
    {
        builder.AddPlatformOrgStructure<OrgUnit, LegalEntity, JobTitle, OrgChartPosition>()
            .UseEntityFramework<OrgUnit, LegalEntity, JobTitle, OrgChartPosition, TUnitOfWork>();

        builder.AddOrgStructureCatalogs<
            OrgUnitType,
            JobGrade,
            JobPosition,
            OrgUnitMember,
            OrgUnit,
            JobTitle,
            LegalEntity>();

        builder.Services.AddOrgStructureApi();

        builder.Services.AddScoped<IOrgStructureDbContextProvider>(serviceProvider =>
            new PlatformOrgStructureDbContextProvider(serviceProvider.GetRequiredService<TTenantContext>()));
        builder.AddCurrentTenant<TenantCatalogProfile>();
        builder.Services.TryAddScoped<ICurrentTenantStore<TenantCatalogProfile>, TenantCatalogCurrentTenantStore>();

        return builder;
    }
}
