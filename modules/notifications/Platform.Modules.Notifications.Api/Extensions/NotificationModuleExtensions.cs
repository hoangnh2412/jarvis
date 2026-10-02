using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Module.Notifications.Controllers;

namespace Module.Notifications.Extensions;

public static class NotificationModuleExtensions
{
    /// <summary>
    /// Đăng ký ApplicationPart của Module.Notifications.
    /// Không gọi AddCoreRealtime / UseSignalR / MapRealtimeHub — Host tự gọi.
    /// Gọi sau <c>AddControllers()</c> của host (vd. <c>AddCoreWebApi()</c>).
    /// </summary>
    public static IHostApplicationBuilder AddNotificationModule(this IHostApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.Services.TryAddEnumerable(
            ServiceDescriptor.Singleton<IConfigureOptions<MvcOptions>, NotificationModuleMvcInitializer>());

        return builder;
    }

    private sealed class NotificationModuleMvcInitializer(ApplicationPartManager applicationPartManager)
        : IConfigureOptions<MvcOptions>
    {
        public void Configure(MvcOptions options)
        {
            var assembly = typeof(NotificationsController).Assembly;
            if (applicationPartManager.ApplicationParts.OfType<AssemblyPart>()
                    .Any(p => p.Assembly == assembly))
            {
                return;
            }

            applicationPartManager.ApplicationParts.Add(new AssemblyPart(assembly));
        }
    }
}
