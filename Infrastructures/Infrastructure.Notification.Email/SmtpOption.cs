using MailKit.Security;

namespace Infrastructure.Notification.Email
{
    public class SmtpOption
    {
        public string Host { get; set; }
        public int Port { get; set; }
        public SecureSocketOptions Socket { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string From { get; set; }
        public string FromName { get; set; }
        public bool Authentication { get; set; }
        public bool WithoutSsl { get; set; }
    }
}