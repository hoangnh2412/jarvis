using Microsoft.Extensions.DependencyInjection;
using Platform.Modules.Setting.API.Controllers;
using Platform.Modules.Setting.Extensions;

namespace Platform.Modules.Setting.API.Extensions;

/// <summary>
/// Đăng ký HTTP API sẵn của module Setting vào host ASP.NET Core (opt-in).
/// </summary>
public static class SettingApiExtensions
{
    /// <summary>
    /// Expose controller CRUD/form/groups chuẩn tại <c>api/v{version}/settings</c>.
    /// Host vẫn có thể thêm endpoint riêng (vd. test email) trong app của mình.
    /// Không đăng ký Authorize — phù hợp hệ thống nội bộ; host tự bảo vệ nếu cần.
    /// </summary>
    /// <param name="builder">Builder fluent sau <c>AddCoreSetting()</c>.</param>
    /// <returns>Chính builder để gọi fluent tiếp.</returns>
    public static PlatformSettingBuilder UseHttpApi(this PlatformSettingBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        // AddControllers đã gọi ở host (vd. AddCoreJson) — gọi lại để lấy IMvcBuilder và gắn ApplicationPart.
        builder.HostBuilder.Services
            .AddControllers()
            .AddApplicationPart(typeof(SettingController).Assembly);

        return builder;
    }
}
