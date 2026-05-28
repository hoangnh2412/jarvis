using Jarvis.Notification;
using Jarvis.Notification.Mailkit;

// Trong HostLayerExtension hoặc Program.cs
public static IHostApplicationBuilder AddNotificationLayer(this IHostApplicationBuilder builder)
{
    var notificationBuilder = new NotificationBuilder(builder.Services);
    notificationBuilder
        .AddSmtp(builder.Configuration.GetSection("Smtp"))
        .AddEmailSender<MailkitSender>();
    return builder;
}
