using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Jarvis.Modules.Setting.API.Controllers;

namespace Sample.Settings;

/// <summary>
/// Mẫu hướng dẫn: module Setting <b>không</b> gắn <c>[Authorize]</c>.
/// Host muốn bảo vệ HTTP API thì tự thêm auth ở pipeline + convention/policy như dưới.
/// File này chỉ là mẫu — <b>không</b> được gọi từ <c>Program.cs</c> / <c>AddSampleSettings</c> mặc định.
/// </summary>
/// <remarks>
/// <para>
/// Luồng khuyến nghị khi cần Auth:
/// </para>
/// <list type="number">
/// <item><c>builder.Services.AddAuthorization(...)</c> + đăng ký scheme (Sample đã có <c>AddSampleAuthentication</c>).</item>
/// <item><c>app.UseAuthentication()</c> rồi <c>app.UseAuthorization()</c> (thứ tự quan trọng).</item>
/// <item>Gắn <see cref="AuthorizeFilter"/> lên <see cref="SettingController"/> qua convention (xem <see cref="ApplyAuthorizeToSettingHttpApi"/>).</item>
/// </list>
/// <para>
/// Không sửa package <c>Jarvis.Modules.Setting.API</c> — giữ module trung lập, host quyết định policy.
/// </para>
/// </remarks>
public static class SettingHttpApiAuthorizationSample
{
    /// <summary>
    /// Policy mẫu cho API quản trị cấu hình (đổi tên/role theo host).
    /// </summary>
    public const string SettingAdminPolicy = "SettingAdmin";

    /// <summary>
    /// Đăng ký policy + convention gắn <c>[Authorize]</c> lên controller Setting từ ApplicationPart.
    /// Gọi <b>sau</b> <c>AddSampleSettings()</c> / <c>UseHttpApi()</c>.
    /// </summary>
    /// <example>
    /// <code>
    /// // Program.cs — chỉ bật khi host thật sự cần Auth cho Setting API
    /// builder.AddSampleSettings();
    /// builder.Services.AddAuthorization(options =>
    /// {
    ///     options.AddPolicy(SettingHttpApiAuthorizationSample.SettingAdminPolicy, policy =>
    ///         policy.RequireAuthenticatedUser());
    ///     // hoặc: policy.RequireRole("Admin");
    /// });
    /// SettingHttpApiAuthorizationSample.ApplyAuthorizeToSettingHttpApi(builder.Services);
    ///
    /// // ...
    /// app.UseAuthentication();
    /// app.UseAuthorization(); // bắt buộc nếu dùng AuthorizeFilter
    /// app.MapControllers();
    /// </code>
    /// </example>
    public static void ApplyAuthorizeToSettingHttpApi(IServiceCollection services, string? policyName = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        var policy = string.IsNullOrWhiteSpace(policyName) ? SettingAdminPolicy : policyName;

        services.AddControllers().AddMvcOptions(options =>
        {
            options.Conventions.Add(new AuthorizeControllerByTypeConvention(
                typeof(SettingController),
                policy));
        });
    }

    /// <summary>
    /// Convention: mọi action của controller chỉ định nhận <see cref="AuthorizeFilter"/>.
    /// </summary>
    private sealed class AuthorizeControllerByTypeConvention(Type controllerType, string policyName)
        : IControllerModelConvention
    {
        public void Apply(ControllerModel controller)
        {
            if (controller.ControllerType != controllerType)
                return;

            controller.Filters.Add(new AuthorizeFilter(policyName));
        }
    }
}
