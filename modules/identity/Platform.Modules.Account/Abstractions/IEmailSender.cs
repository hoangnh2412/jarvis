namespace Platform.Modules.Account.Abstractions;

/// <summary>
/// Gửi email liên quan đến tài khoản (self-service). Account chỉ phụ thuộc vào abstraction này;
/// implementation thật (SMTP/SendGrid...) là quyết định hạ tầng riêng — chưa chốt ADR, xem
/// <see cref="Services.LoggingEmailSender"/> cho implementation tạm thời (log-only).
/// </summary>
public interface IEmailSender
{
    /// <summary>
    /// Gửi link/đường dẫn reset password tới email người dùng.
    /// </summary>
    Task SendPasswordResetEmailAsync(string email, string resetToken, CancellationToken cancellationToken = default);
}
