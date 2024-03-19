using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace Jarvis.Infrastructure.Emailing.Mailkit;

public class MailkitSender : IEmailSender
{
    private readonly SmtpOption _options;

    public MailkitSender(
        IOptions<SmtpOption> options)
    {
        if (options != null)
            _options = options.Value;
    }

    public async Task SendAsync(string subject, string content, string to)
    {
        await SendAsync(_options, subject, content, to, null, null, null);
    }

    public async Task SendAsync(string subject, string content, string to, Dictionary<string, byte[]> attachments)
    {
        await SendAsync(_options, subject, content, to, null, null, attachments);
    }

    public async Task SendAsync(string subject, string content, string to, string[] cc, string[] bcc)
    {
        await SendAsync(_options, subject, content, to, cc, bcc, null);
    }

    public async Task SendAsync(string subject, string content, string to, string[] cc, string[] bcc, Dictionary<string, byte[]> attachments)
    {
        await SendAsync(_options, subject, content, to, cc, bcc, attachments);
    }

    public async Task SendAsync(SmtpOption option, string subject, string content, string to)
    {
        await SendAsync(option, subject, content, to, null, null, null);
    }

    public async Task SendAsync(SmtpOption option, string subject, string content, string to, Dictionary<string, byte[]> attachments)
    {
        await SendAsync(option, subject, content, to, null, null, attachments);
    }

    public async Task SendAsync(SmtpOption option, string subject, string content, string to, string[] cc, string[] bcc)
    {
        await SendAsync(option, subject, content, to, cc, bcc, null);
    }

    public async Task SendAsync(SmtpOption option, string subject, string content, string to, string[] cc, string[] bcc, Dictionary<string, byte[]> attachments)
    {
        var message = GenerateMessage(option, subject, content, to, cc, bcc, attachments);

        using (var client = new SmtpClient())
        {
            if (option.WithoutSsl)
                client.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;

            await client.ConnectAsync(option.Host, option.Port, (SecureSocketOptions)option.Socket);
            await client.AuthenticateAsync(option.UserName, option.Password);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
    }

    public virtual MimeMessage GenerateMessage(SmtpOption option, string subject, string content, string to, string[] cc, string[] bcc, Dictionary<string, byte[]> attachments)
    {
        var message = new MimeMessage();
        message.Subject = subject;
        message.From.Add(new MailboxAddress(option.FromName, option.From));
        message.To.Add(MailboxAddress.Parse(to));

        if (cc != null && cc.Length > 0)
        {
            foreach (var item in cc)
            {
                if (InternetAddress.TryParse(item, out InternetAddress addressCc))
                    message.Cc.Add(addressCc);
            }
        }

        if (bcc != null && bcc.Length > 0)
        {
            foreach (var item in bcc)
            {
                if (InternetAddress.TryParse(item, out InternetAddress addressBcc))
                    message.Bcc.Add(addressBcc);
            }
        }

        var bodyBuilder = new BodyBuilder { HtmlBody = content };
        if (attachments != null && attachments.Count > 0)
        {
            foreach (var item in attachments)
            {
                bodyBuilder.Attachments.Add(item.Key, item.Value, new ContentType("", ""));
            }
        }
        message.Body = bodyBuilder.ToMessageBody();
        return message;
    }
}