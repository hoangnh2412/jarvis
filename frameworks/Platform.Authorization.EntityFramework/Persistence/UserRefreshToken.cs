namespace Platform.Authorization.EntityFramework.Persistence;

/// <summary>Refresh token đã băm. Giá trị gửi cho client không lưu trên bảng này.</summary>
public sealed class UserRefreshToken
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public string TokenHash { get; set; } = string.Empty;

    public DateTime ExpiresAtUtc { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public DateTime? RevokedAtUtc { get; set; }
}
