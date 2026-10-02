using Platform.DDD.Domain.Services;

namespace Platform.Multitenancy;

/// <summary>
/// <see cref="ICurrentTenantIdentity"/> mặc định cho host không cần kiểu profile tùy chỉnh.
/// </summary>
public class CurrentTenantInfo : ICurrentTenantIdentity
{
    public Guid? TenantId { get; init; }

    public string? Name { get; init; }
}
