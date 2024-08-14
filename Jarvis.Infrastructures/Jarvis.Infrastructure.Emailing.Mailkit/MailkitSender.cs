using System.Net.Mail;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace Jarvis.Infrastructure.Emailing.Mailkit;

public class MailkitSender : BaseSmtpSender, IEmailSender
{
    public MailkitSender(
        IOptions<SmtpOption> options)
        : base(options)
    {
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

    public override Task SendAsync<T>(string subject, string content, string to, string[] cc, string[] bcc, Attachment[] attachments, T option = default)
    {
        return SendAsync<T>(subject, content, new string[1] { to }, cc, bcc, attachments, option);
    }

    public override async Task SendAsync<T>(string subject, string content, string[] to, string[] cc, string[] bcc, Attachment[] attachments, T option = default)
    {
        var smtpOption = option as SmtpOption;
        var message = GenerateMessage(smtpOption, subject, content, to, cc, bcc, attachments);

        using (var client = new MailKit.Net.Smtp.SmtpClient())
        {
            if (smtpOption.WithoutSsl)
                client.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;

            await client.ConnectAsync(smtpOption.Host, smtpOption.Port, (SecureSocketOptions)smtpOption.Socket);
            await client.AuthenticateAsync(smtpOption.UserName, smtpOption.Password);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
    }

    public virtual MimeMessage GenerateMessage(SmtpOption option, string subject, string content, string[] to, string[] cc, string[] bcc, Attachment[] attachments)
    {
        var message = new MimeMessage();
        message.Subject = subject;
        message.From.Add(new MailboxAddress(option.FromName, option.From));

        foreach (var item in to)
        {
            message.To.Add(MailboxAddress.Parse(item));
        }

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
        if (attachments != null && attachments.Length > 0)
        {
            foreach (var item in attachments)
            {
                var memoryStream = item.ContentStream as MemoryStream;
                var fileContent = new ByteArrayContent(memoryStream.ToArray());

                bodyBuilder.Attachments.Add(item.Name, item.ContentStream, ContentType.Parse(item.ContentType.MediaType));
            }
        }
        message.Body = bodyBuilder.ToMessageBody();
        return message;
    }
}