using System.Net.Mail;

namespace Jarvis.Infrastructure.Emailing;

/// <summary>
/// THe interface abstract the mail handling functions
/// </summary>
public interface IEmailSender
{
    Task SendAsync<T>(MailMessage message, T option = default);

    Task SendAsync<T>(string subject, string content, string[] to, string[] cc, string[] bcc, Attachment[] attachments, T option = default);

    Task SendAsync<T>(string subject, string content, string to, string[] cc, string[] bcc, Attachment[] attachments, T option = default);
}