using Jarvis.Modules.Notifications.Definitions;
using Jarvis.Modules.Setting.API.Extensions;
using Jarvis.Modules.Setting.EntityFramework.Extensions;
using Jarvis.Modules.Setting.Extensions;
using Jarvis.Multitenancy;
using Sample.Persistence;
using Sample.Settings;

namespace Sample.Extensions;

/// <summary>Registers the Setting module and all definitions owned by the Sample application.</summary>
/// <remarks>
/// HTTP API Setting không kèm Auth (module jarvis). Nếu host cần bảo vệ endpoint,
/// xem mẫu <see cref="SettingHttpApiAuthorizationSample"/> — không bật mặc định trong Sample.
/// </remarks>
public static class SampleSettingExtensions
{
    public static WebApplicationBuilder AddSampleSettings(this WebApplicationBuilder builder)
    {
        // Cache item "Setting" cấu hình qua Cache:Items (Jarvis.Caching).
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
