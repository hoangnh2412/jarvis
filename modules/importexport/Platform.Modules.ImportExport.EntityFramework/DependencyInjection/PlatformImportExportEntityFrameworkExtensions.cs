using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Platform.DDD.Domain.Repositories;
using Platform.DDD.Domain.Services;
using Platform.Modules.ImportExport.DependencyInjection;
using Platform.Modules.ImportExport.EntityFramework.Entities;
using Platform.Modules.ImportExport.Application.Import;
using Platform.Modules.ImportExport.Application.Common.Validation;
using Platform.Modules.ImportExport.EntityFramework.Services;
using Platform.Modules.ImportExport.EntityFramework.Stores;

namespace Platform.Modules.ImportExport.EntityFramework.Extensions;

/// <summary>
/// Đăng ký entity Excel template mặc định với template store qua Entity Framework.
/// </summary>
public static class ImportExportEntityFrameworkExtensions
{
    /// <summary>
    /// Dùng entity <see cref="ExcelTemplate"/> đi kèm package và đăng ký store EF cho Unit of Work của host.
    /// </summary>
    public static PlatformImportExportBuilder UseEntityFramework<TUnitOfWork, TTenant>(
        this PlatformImportExportBuilder builder)
        where TUnitOfWork : class, IUnitOfWork
        where TTenant : class, ICurrentTenantIdentity
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.HostBuilder.Services
            .AddPlatformImportExportEntityFramework<ExcelTemplate, TUnitOfWork, TTenant>();
        return builder;
    }

    /// <summary>
    /// Đăng ký store EF với entity template do host cung cấp.
    /// </summary>
    public static PlatformImportExportBuilder UseEntityFramework<TEntity, TUnitOfWork, TTenant>(
        this PlatformImportExportBuilder builder)
        where TEntity : class, IExcelTemplateEntity, Platform.DDD.Domain.Entities.IEntity
        where TUnitOfWork : class, IUnitOfWork
        where TTenant : class, ICurrentTenantIdentity
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.HostBuilder.Services
            .AddPlatformImportExportEntityFramework<TEntity, TUnitOfWork, TTenant>();
        return builder;
    }

    /// <summary>
    /// Registers EF template store. Host must register <see cref="ICurrentTenant{TTenant}"/>.
    /// </summary>
    public static IServiceCollection AddPlatformImportExportEntityFramework<TEntity, TUnitOfWork, TTenant>(
        this IServiceCollection services)
        where TEntity : class, IExcelTemplateEntity, Platform.DDD.Domain.Entities.IEntity
        where TUnitOfWork : class, IUnitOfWork
        where TTenant : class, ICurrentTenantIdentity
    {
        services.TryAddSingleton<TemplateFileResolver>();
        services.TryAddScoped<IUniqueValueChecker, DefaultUniqueValueChecker>();
        services.TryAddScoped<IImportDataPersister, DefaultImportDataPersister>();
        services.TryAddScoped<ITemplateStore, DbTemplateStore<TEntity, TUnitOfWork, TTenant>>();
        services.TryAddScoped<ITemplateService, TemplateService>();
        return services;
    }

    /// <summary>
    /// Registers folder-based template store. Host must register <see cref="ICurrentTenant{TTenant}"/>.
    /// </summary>
    public static IServiceCollection AddPlatformImportExportFolderTemplates<TTenant>(this IServiceCollection services)
        where TTenant : class, ICurrentTenantIdentity
    {
        services.TryAddSingleton<TemplateFileResolver>();
        services.TryAddScoped<IUniqueValueChecker, DefaultUniqueValueChecker>();
        services.TryAddScoped<IImportDataPersister, DefaultImportDataPersister>();
        services.TryAddScoped<ITemplateStore, FolderTemplateStore<TTenant>>();
        services.TryAddScoped<ITemplateService, TemplateService>();
        return services;
    }
}
