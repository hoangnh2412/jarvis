using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Platform.ImportExport.MiniExcel.Processors;
using Platform.ImportExport.MiniExcel.Validation;

namespace Platform.ImportExport.MiniExcel.DependencyInjection;

public static class PlatformImportExportMiniExcelExtensions
{
    public static IServiceCollection AddPlatformImportExportMiniExcel(this IServiceCollection services)
    {
        services.TryAddScoped<IImportRequestValidator, ImportRequestValidator>();
        services.TryAddScoped<IRowValidator, DefaultRowValidator>();
        services.TryAddScoped<IImportProcessor, MiniExcelImportProcessor>();
        services.TryAddScoped<IExportProcessor, MiniExcelExportProcessor>();
        return services;
    }
}
