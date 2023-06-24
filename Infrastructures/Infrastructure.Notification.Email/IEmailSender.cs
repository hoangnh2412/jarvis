using System.Collections.Generic;
using System.Threading.Tasks;

namespace Infrastructure.Notification.Email
{
    public interface IEmailSender
    {
        Task SendAsync(SmtpOption option, string subject, string content, string to, string[] cc = null, string[] bcc = null, Dictionary<string, byte[]> attachments = null);
    }
}