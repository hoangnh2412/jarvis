namespace Jarvis.DDD.Domain.Services;

/// <summary>
/// Identity tối thiểu từ token/ambient. Host tự implement profile và thêm field riêng.
/// </summary>
public interface ICurrentUserIdentity
{
    Guid? UserId { get; }

    Guid? TokenId { get; }

    /// <summary>
    /// Tenant thuộc về (home / belonging) — tenant mà user thuộc về.
    /// Không đổi khi <see cref="ICurrentTenant{TTenant}.Change"/> chuyển tenant đang làm việc.
    /// </summary>
    Guid? TenantId { get; }
}
