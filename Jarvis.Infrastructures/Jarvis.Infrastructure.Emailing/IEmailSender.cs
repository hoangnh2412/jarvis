namespace Jarvis.Infrastructure.Emailing;

/// <summary>
/// THe interface abstract the mail handling functions
/// </summary>
public interface IEmailSender
{
    /// <summary>
    /// Send mail with general setting from appsettings.json
    /// </summary>
    /// <param name="subject"></param>
    /// <param name="content"></param>
    /// <param name="to"></param>
    /// <returns></returns>
    Task SendAsync(string subject, string content, string to);

    /// <summary>
    /// Send mail with general setting from appsettings.json
    /// </summary>
    /// <param name="subject"></param>
    /// <param name="content"></param>
    /// <param name="to"></param>
    /// <param name="attachments"></param>
    /// <returns></returns>
    Task SendAsync(string subject, string content, string to, Dictionary<string, byte[]> attachments);

    /// <summary>
    /// Send mail with general setting from appsettings.json
    /// </summary>
    /// <param name="subject"></param>
    /// <param name="content"></param>
    /// <param name="to"></param>
    /// <param name="cc"></param>
    /// <param name="bcc"></param>
    /// <returns></returns>
    Task SendAsync(string subject, string content, string to, string[] cc, string[] bcc);

    /// <summary>
    /// Send mail with general setting from appsettings.json
    /// </summary>
    /// <param name="subject"></param>
    /// <param name="content"></param>
    /// <param name="to"></param>
    /// <param name="cc"></param>
    /// <param name="bcc"></param>
    /// <param name="attachments"></param>
    /// <returns></returns>
    Task SendAsync(string subject, string content, string to, string[] cc, string[] bcc, Dictionary<string, byte[]> attachments);

    /// <summary>
    /// Send mail with specific setting
    /// </summary>
    /// <param name="option"></param>
    /// <param name="subject"></param>
    /// <param name="content"></param>
    /// <param name="to"></param>
    /// <returns></returns>
    Task SendAsync(SmtpOption option, string subject, string content, string to);

    /// <summary>
    /// Send mail with specific setting
    /// </summary>
    /// <param name="option"></param>
    /// <param name="subject"></param>
    /// <param name="content"></param>
    /// <param name="to"></param>
    /// <param name="attachments"></param>
    /// <returns></returns>
    Task SendAsync(SmtpOption option, string subject, string content, string to, Dictionary<string, byte[]> attachments);

    /// <summary>
    /// Send mail with specific setting
    /// </summary>
    /// <param name="option"></param>
    /// <param name="subject"></param>
    /// <param name="content"></param>
    /// <param name="to"></param>
    /// <param name="cc"></param>
    /// <param name="bcc"></param>
    /// <returns></returns>
    Task SendAsync(SmtpOption option, string subject, string content, string to, string[] cc, string[] bcc);

    /// <summary>
    /// Send mail with specific setting
    /// </summary>
    /// <param name="option"></param>
    /// <param name="subject"></param>
    /// <param name="content"></param>
    /// <param name="to"></param>
    /// <param name="cc"></param>
    /// <param name="bcc"></param>
    /// <param name="attachments"></param>
    /// <returns></returns>
    Task SendAsync(SmtpOption option, string subject, string content, string to, string[] cc, string[] bcc, Dictionary<string, byte[]> attachments);
}