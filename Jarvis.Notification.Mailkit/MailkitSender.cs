using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace Jarvis.Notification.Mailkit;

public class MailkitSender(
    IOptions<SmtpOption> options) : IEmailSender
{
    private readonly SmtpOption _options = options.Value;

    public async Task SendAsync(string subject, string content, string[] to, string[]? cc = null, string[]? bcc = null, Attachment[]? attachments = null)
    {
        var message = GenerateMessage(subject, content, to, cc, bcc, attachments);

        using var client = new SmtpClient();
        if (_options.WithoutSsl)
            client.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;

        await client.ConnectAsync(_options.Host, _options.Port, (SecureSocketOptions)_options.Socket);
        await client.AuthenticateAsync(_options.UserName, _options.Password);
        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }

    public virtual MimeMessage GenerateMessage(string subject, string content, string[] to, string[]? cc = null, string[]? bcc = null, Attachment[]? attachments = null)
    {
        var message = new MimeMessage
        {
            Subject = subject
        };
        message.From.Add(new MailboxAddress(_options.FromName, _options.From));

        foreach (var item in to)
        {
            if (InternetAddress.TryParse(item, out InternetAddress addressTo))
                message.To.Add(addressTo);
        }

        if (_options.To != null && _options.To.Length > 0)
        {
            foreach (var item in _options.To)
            {
                if (InternetAddress.TryParse(item, out InternetAddress addressTo))
                    message.To.Add(addressTo);
            }
        }

        if (cc != null && cc.Length > 0)
        {
            foreach (var item in cc)
            {
                if (InternetAddress.TryParse(item, out InternetAddress addressCc))
                    message.Cc.Add(addressCc);
            }
        }

        if (_options.Cc != null && _options.Cc.Length > 0)
        {
            foreach (var item in _options.Cc)
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

        if (_options.Bcc != null && _options.Bcc.Length > 0)
        {
            foreach (var item in _options.Bcc)
            {
                if (InternetAddress.TryParse(item, out InternetAddress addressBcc))
                    message.Bcc.Add(addressBcc);
            }
        }

        var bodyBuilder = new BodyBuilder { HtmlBody = content };
        if (attachments != null && attachments.Length > 0)
        {
            foreach (var item in attachments)
            {
                var contentType = new ContentType(item.ContentType.MediaType, item.ContentType.CharSet);

                if (item.DataContent != null)
                    bodyBuilder.Attachments.Add(item.FileName, item.DataContent, contentType);

                if (item.StreamContent != null)
                    bodyBuilder.Attachments.Add(item.FileName, item.StreamContent, contentType);
            }
        }
        message.Body = bodyBuilder.ToMessageBody();
        return message;
    }
}
