using Microsoft.Extensions.DependencyInjection;
using Platform.Modules.ImportExport.API.Controllers;
using Platform.Modules.ImportExport.DependencyInjection;

namespace Platform.Modules.ImportExport.API.Extensions;

/// <summary>
/// Đăng ký HTTP API sẵn của module ImportExport vào host ASP.NET Core (opt-in).
/// </summary>
public static class ImportExportApiExtensions
{
    /// <summary>
    /// Expose controllers template CRUD, entity schema, và import/export Excel chuẩn.
    /// Không đăng ký Authorize — host tự bảo vệ nếu cần (giống module Setting).
    /// </summary>
    public static PlatformImportExportBuilder UseHttpApi(this PlatformImportExportBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.HostBuilder.Services
            .AddControllers()
            .AddApplicationPart(typeof(TemplatesController).Assembly);

        return builder;
    }
}
