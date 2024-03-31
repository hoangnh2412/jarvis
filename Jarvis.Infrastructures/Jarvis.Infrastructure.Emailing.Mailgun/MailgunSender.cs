using System.Net.Mail;
using Microsoft.Extensions.Options;

namespace Jarvis.Infrastructure.Emailing.Mailgun;

public class MailgunSender : BaseHttpSender, IEmailSender
{
    private readonly IMailgunClient _client;

    public MailgunSender(
        IMailgunClient client,
        IOptions<HttpOption> options)
        : base(options)
    {
        _client = client;
    }

    public override async Task SendAsync<T>(MailMessage message, T option = default)
    {
        await SendAsync(
            message.Subject,
            message.Body,
            message.To[0].Address,
            message.CC.Select(x => x.Address).ToArray(),
            message.Bcc.Select(x => x.Address).ToArray(),
            message.Attachments.ToArray(),
            option);
    }

    public override async Task SendAsync<T>(string subject, string content, string[] to, string[] cc, string[] bcc, Attachment[] attachments, T option = default)
    {
        await _client.SendAsync(subject, content, to, cc, bcc, attachments, option);
    }
}
