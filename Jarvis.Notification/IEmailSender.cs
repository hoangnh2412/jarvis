namespace Jarvis.Notification;

public interface IEmailSender
{
    Task SendAsync(string subject, string content, string[] to, string[]? cc = null, string[]? bcc = null, Attachment[]? attachments = null);
}