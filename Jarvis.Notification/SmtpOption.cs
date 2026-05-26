namespace Jarvis.Notification;

public class SmtpOption
{
    public required string Host { get; set; }
    public int Port { get; set; }
    public int Socket { get; set; }
    public required string UserName { get; set; }
    public required string Password { get; set; }
    public required string From { get; set; }
    public required string FromName { get; set; }
    public bool Authentication { get; set; }
    public bool WithoutSsl { get; set; }
    public string[] To { get; set; } = [];
    public string[] Cc { get; set; } = [];
    public string[] Bcc { get; set; } = [];
}
