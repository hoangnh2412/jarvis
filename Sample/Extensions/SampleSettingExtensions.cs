using Platform.Modules.Notifications.Definitions;
using Platform.Modules.Setting.API.Extensions;
using Platform.Modules.Setting.EntityFramework.Extensions;
using Platform.Modules.Setting.Extensions;
using Platform.Multitenancy;
using Sample.Persistence;
using Sample.Settings;

namespace Sample.Extensions;

/// <summary>Registers the Setting module and all definitions owned by the Sample application.</summary>
/// <remarks>
/// HTTP API Setting không kèm Auth (module platform). Nếu host cần bảo vệ endpoint,
/// xem mẫu <see cref="SettingHttpApiAuthorizationSample"/> — không bật mặc định trong Sample.
/// </remarks>
public static class SampleSettingExtensions
{
    public static WebApplicationBuilder AddSampleSettings(this WebApplicationBuilder builder)
    {
        // Cache item "Setting" cấu hình qua Cache:Items (Platform.Caching).
        // Encryption:DataEncryptionKey + Cache:Items:Setting nằm trong Sample/appsettings.json.
        builder.AddCoreSetting()
            .UseEntityFramework<IMasterUnitOfWork, CurrentTenantInfo>()
            .UseHttpApi()
            .AddProvider<DemoSettingDefinition>()
            .AddProvider<EmailSettingDefinition>()
            .AddProvider<SmsSettingDefinition>()
            .AddProvider<NotificationSettingDefinition>()
            .AddProvider<NotificationInboxSettingDefinition>()
            .AddProvider<TimezoneDefinition>()
            .AddProvider<LocalizationSettingDefinition>();

        // Auth mẫu (không bật):
        // builder.Services.AddAuthorization(o =>
        //     o.AddPolicy(SettingHttpApiAuthorizationSample.SettingAdminPolicy, p => p.RequireAuthenticatedUser()));
        // SettingHttpApiAuthorizationSample.ApplyAuthorizeToSettingHttpApi(builder.Services);
        // và trong pipeline: app.UseAuthorization() sau UseAuthentication().

        return builder;
    }
}
