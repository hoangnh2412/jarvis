namespace Platform.Modules.Account.Services;

using Platform.Modules.Account.Abstractions;
using Microsoft.Extensions.Logging;

/// <summary>
/// Implementation tạm thời của <see cref="IEmailSender"/>: chỉ log, không gửi email thật.
/// Nợ kỹ thuật: cần ADR chọn nhà cung cấp email (SMTP/SendGrid/...) trước khi thay bằng
/// implementation thật (ví dụ dùng <c>Platform.Notification.Mailkit</c>).
/// </summary>
public sealed class LoggingEmailSender(ILogger<LoggingEmailSender> logger) : IEmailSender
{
    private readonly ILogger<LoggingEmailSender> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public Task SendPasswordResetEmailAsync(string email, string resetToken, CancellationToken cancellationToken = default)
    {
        _logger.LogWarning(
            "Email sending chưa cấu hình (nợ kỹ thuật). Reset password token cho {Email} đã được tạo nhưng không gửi được.",
            email);
        return Task.CompletedTask;
    }
}
