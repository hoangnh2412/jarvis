using System.Net.Mail;
using Microsoft.Extensions.Options;

namespace Jarvis.Infrastructure.Emailing;

public class BaseSmtpSender : IEmailSender
{
    private readonly SmtpOption _options;

    public BaseSmtpSender(
        IOptions<SmtpOption> options)
    {
        _options = options.Value;
    }

    public virtual async Task SendAsync<T>(MailMessage message, T option = default)
    {
        var smtpOption = _options;
        if (!option.Equals(default))
            smtpOption = option as SmtpOption;

        using (var client = new SmtpClient(smtpOption.Host, smtpOption.Port))
        {
            if (!smtpOption.WithoutSsl)
                client.EnableSsl = true;

            client.Credentials = new System.Net.NetworkCredential(smtpOption.UserName, smtpOption.Password);
            await client.SendMailAsync(message);
        }
    }

    public virtual async Task SendAsync<T>(string subject, string content, string[] to, string[] cc, string[] bcc, Attachment[] attachments, T option = default)
    {
        var smtpOption = _options;
        if (!option.Equals(default))
            smtpOption = option as SmtpOption;

        using (MailMessage message = new MailMessage())
        {
            message.From = new MailAddress(smtpOption.From, smtpOption.FromName);

            foreach (var item in to)
            {
                message.To.Add(new MailAddress(item));
            }

            message.Subject = subject;
            message.Body = content;
            message.IsBodyHtml = true;
            message.BodyEncoding = System.Text.Encoding.UTF8;
            message.SubjectEncoding = System.Text.Encoding.UTF8;
            message.Priority = MailPriority.Normal;

            if (cc != null && cc.Length > 0)
            {
                foreach (var item in cc)
                {
                    message.CC.Add(new MailAddress(item));
                }
            }

            if (bcc != null && bcc.Length > 0)
            {
                foreach (var item in bcc)
                {
                    message.Bcc.Add(new MailAddress(item));
                }
            }

            await SendAsync(message, option);
        }
    }

    public virtual async Task SendAsync<T>(string subject, string content, string to, string[] cc, string[] bcc, Attachment[] attachments, T option = default)
    {
        await SendAsync<T>(subject, content, new string[1] { to }, cc, bcc, attachments, option);
    }
}