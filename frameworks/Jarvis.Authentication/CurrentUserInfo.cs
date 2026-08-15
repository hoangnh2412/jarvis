using Jarvis.DDD.Domain.Services;

namespace Jarvis.Authentication;

/// <summary>
/// <see cref="ICurrentUserIdentity"/> mặc định cho host không cần kiểu profile tùy chỉnh.
/// </summary>
public class CurrentUserInfo : ICurrentUserIdentity
{
    public Guid? UserId { get; init; }

    public Guid? TokenId { get; init; }

    /// <inheritdoc cref="ICurrentUserIdentity.TenantId"/>
    public Guid? TenantId { get; init; }

    public string? UserName { get; init; }
}
