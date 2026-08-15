namespace Jarvis.DDD.Domain.Services;

/// <summary>
/// Identity tối thiểu của tenant đang làm việc. Host tự implement profile và thêm field riêng.
/// </summary>
public interface ICurrentTenantIdentity
{
    Guid? TenantId { get; }
}
