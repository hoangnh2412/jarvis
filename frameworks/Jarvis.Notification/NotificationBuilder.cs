using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Jarvis.Notification;

public class NotificationBuilder(
    IServiceCollection services)
{
    private readonly IServiceCollection _services = services;
    public SmtpOption? SmtpOption;

    public NotificationBuilder AddSmtp(IConfigurationSection configureOptions)
    {
        _services.Configure<SmtpOption>(configureOptions);

        var smtpOption = configureOptions.Get<SmtpOption>();
        if (smtpOption == null)
            return this;

        SmtpOption = smtpOption;
        return this;
    }

    public NotificationBuilder AddEmailSender<T>() where T : IEmailSender
    {
        _services.AddKeyedSingleton(typeof(IEmailSender), typeof(T).Name, typeof(T));
        return this;
    }
}