using System.Net.Mail;

namespace Jarvis.Infrastructure.Emailing.Mailgun;

public interface IMailgunClient
{
    Task SendAsync<T>(string subject, string content, string to, string[] cc, string[] bcc, Attachment[] attachments, T option = default);

    Task SendAsync<T>(string subject, string content, string[] to, string[] cc, string[] bcc, Attachment[] attachments, T option = default);
}