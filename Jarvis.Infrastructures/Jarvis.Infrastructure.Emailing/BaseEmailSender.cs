using Microsoft.Extensions.Options;

namespace Jarvis.Infrastructure.Emailing;

public abstract class BaseEmailSender : IEmailSender
{
    private readonly SmtpOption _options;

    public BaseEmailSender(
        IOptions<SmtpOption> options)
    {
        _options = options.Value;
    }

    public async Task SendAsync(string subject, string content, string to)
    {
        await SendAsync(_options, subject, content, to, null, null, null);
    }

    public async Task SendAsync(string subject, string content, string to, Dictionary<string, byte[]> attachments)
    {
        await SendAsync(_options, subject, content, to, null, null, attachments);
    }

    public async Task SendAsync(string subject, string content, string to, string[] cc, string[] bcc)
    {
        await SendAsync(_options, subject, content, to, cc, bcc, null);
    }

    public async Task SendAsync(string subject, string content, string to, string[] cc, string[] bcc, Dictionary<string, byte[]> attachments)
    {
        await SendAsync(_options, subject, content, to, cc, bcc, attachments);
    }

    public async Task SendAsync(SmtpOption option, string subject, string content, string to)
    {
        await SendAsync(option, subject, content, to, null, null, null);
    }

    public async Task SendAsync(SmtpOption option, string subject, string content, string to, Dictionary<string, byte[]> attachments)
    {
        await SendAsync(option, subject, content, to, null, null, attachments);
    }

    public async Task SendAsync(SmtpOption option, string subject, string content, string to, string[] cc, string[] bcc)
    {
        await SendAsync(option, subject, content, to, cc, bcc, null);
    }

    public abstract Task SendAsync(SmtpOption option, string subject, string content, string to, string[] cc, string[] bcc, Dictionary<string, byte[]> attachments);
}