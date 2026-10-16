using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Platform.Modules.ImportExport.Application.Common.Execution;
using Platform.Modules.ImportExport.Application.Common.Options;
using Platform.Modules.ImportExport.Application.Export;
using Platform.Modules.ImportExport.Application.Import;
using Platform.Modules.ImportExport.Application.Schemas;

namespace Platform.Modules.ImportExport.DependencyInjection;

/// <summary>
/// Điểm đăng ký DI của module ImportExport vào host.
/// </summary>
/// <remarks>
/// <code>
/// builder.AddCoreImport()
///     .ConfigureEntities(o =&gt; o.Add&lt;StudentImportRow&gt;("Student", "Học sinh"))
///     .UseMiniExcel()
///     .UseEntityFramework&lt;ITenantUnitOfWork, CurrentTenantInfo&gt;()
///     .UseHttpApi();
/// builder.AddCoreExport();
/// </code>
/// Hoặc đăng ký cả hai: <c>builder.AddCoreImportExport()</c>.
/// </remarks>
public static class PlatformImportExportExtensions
{
    /// <summary>
    /// Đăng ký phần lõi dùng chung + <see cref="IImportService"/>.
    /// Có thể gọi kèm <see cref="AddCoreExport"/> (shared dùng <c>TryAdd</c>).
    /// </summary>
    public static PlatformImportExportBuilder AddCoreImport(this IHostApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        RegisterShared(builder.Services, builder.Configuration);
        builder.Services.TryAddScoped<IImportService, ImportService>();
        return new PlatformImportExportBuilder(builder);
    }

    /// <summary>
    /// Đăng ký phần lõi dùng chung + <see cref="IExportService"/>.
    /// Có thể gọi kèm <see cref="AddCoreImport"/> (shared dùng <c>TryAdd</c>).
    /// </summary>
    public static PlatformImportExportBuilder AddCoreExport(this IHostApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        RegisterShared(builder.Services, builder.Configuration);
        builder.Services.TryAddScoped<IExportService, ExportService>();
        return new PlatformImportExportBuilder(builder);
    }

    /// <summary>
    /// Đăng ký cả import và export (tương đương <see cref="AddCoreImport"/> + <see cref="AddCoreExport"/>).
    /// </summary>
    public static PlatformImportExportBuilder AddCoreImportExport(this IHostApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        AddCoreImport(builder);
        return AddCoreExport(builder);
    }

    public static IServiceCollection AddPlatformImport(this IServiceCollection services)
    {
        RegisterShared(services);
        services.TryAddScoped<IImportService, ImportService>();
        return services;
    }

    public static IServiceCollection AddPlatformExport(this IServiceCollection services)
    {
        RegisterShared(services);
        services.TryAddScoped<IExportService, ExportService>();
        return services;
    }

    public static IServiceCollection AddPlatformImportExport(this IServiceCollection services)
    {
        AddPlatformImport(services);
        return AddPlatformExport(services);
    }

    public static IServiceCollection ConfigureImportExportEntities(
        this IServiceCollection services,
        Action<ImportExportEntityRegistryOptions> configure)
    {
        services.Configure(configure);
        return services;
    }

    private static void RegisterShared(IServiceCollection services, IConfiguration? configuration = null)
    {
        var excelEngineOptions = services.AddOptions<ExcelEngineOptions>();
        if (configuration is not null)
            excelEngineOptions.Bind(configuration.GetSection(ExcelEngineOptions.SectionName));

        services.AddOptions<ImportExportEntityRegistryOptions>();
        services.TryAddSingleton<IImportExportEntityRegistry, ImportExportEntityRegistry>();
        services.TryAddScoped<IImportExportSchemaService, ImportExportSchemaService>();
        services.TryAddScoped<IExecutionExecutor, SyncExecutionExecutor>();
    }
}
