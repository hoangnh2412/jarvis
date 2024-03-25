using Microsoft.Extensions.Options;

namespace Jarvis.Infrastructure.Emailing.Mailgun;

public class MailgunSender : BaseEmailSender, IEmailSender
{
    public MailgunSender(
        IOptions<SmtpOption> options)
        : base(options)
    {
    }

    public override Task SendAsync(SmtpOption option, string subject, string content, string to, string[] cc, string[] bcc, Dictionary<string, byte[]> attachments)
    {
        throw new NotImplementedException();
    }

    // public virtual MimeMessage GenerateMessage(SmtpOption option, string subject, string content, string to, string[] cc, string[] bcc, Dictionary<string, byte[]> attachments)
    // {
    //     var message = new MimeMessage();
    //     message.Subject = subject;
    //     message.From.Add(new MailboxAddress(option.FromName, option.From));
    //     message.To.Add(MailboxAddress.Parse(to));

    //     if (cc != null && cc.Length > 0)
    //     {
    //         foreach (var item in cc)
    //         {
    //             if (InternetAddress.TryParse(item, out InternetAddress addressCc))
    //                 message.Cc.Add(addressCc);
    //         }
    //     }

    //     if (bcc != null && bcc.Length > 0)
    //     {
    //         foreach (var item in bcc)
    //         {
    //             if (InternetAddress.TryParse(item, out InternetAddress addressBcc))
    //                 message.Bcc.Add(addressBcc);
    //         }
    //     }

    //     var bodyBuilder = new BodyBuilder { HtmlBody = content };
    //     if (attachments != null && attachments.Count > 0)
    //     {
    //         foreach (var item in attachments)
    //         {
    //             bodyBuilder.Attachments.Add(item.Key, item.Value, new ContentType("", ""));
    //         }
    //     }
    //     message.Body = bodyBuilder.ToMessageBody();
    //     return message;
    // }
}
