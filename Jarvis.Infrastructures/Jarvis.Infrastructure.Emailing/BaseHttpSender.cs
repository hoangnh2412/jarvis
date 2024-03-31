using System.Net.Mail;
using Microsoft.Extensions.Options;

namespace Jarvis.Infrastructure.Emailing;

public abstract class BaseHttpSender : IEmailSender
{
    private readonly HttpOption _options;

    public BaseHttpSender(
        IOptions<HttpOption> options)
    {
        _options = options.Value;
    }

    public abstract Task SendAsync<T>(MailMessage message, T option = default);

    public abstract Task SendAsync<T>(string subject, string content, string[] to, string[] cc, string[] bcc, Attachment[] attachments, T option = default);

    public virtual async Task SendAsync<T>(string subject, string content, string to, string[] cc, string[] bcc, Attachment[] attachments, T option = default)
    {
        await SendAsync<T>(subject, content, new string[1] { to }, cc, bcc, attachments, option);
    }
}