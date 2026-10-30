using Platform.DDD.Domain.Services;

namespace Platform.Tenants.Services;

/// <summary>Profile ambient đọc từ catalog. Không chứa chuỗi kết nối.</summary>
public sealed class TenantCatalogProfile : ICurrentTenantIdentity
{
    public Guid? TenantId { get; init; }

    public string Code { get; init; } = string.Empty;

    public string Name { get; init; } = string.Empty;

    public Guid? ParentId { get; init; }

    public bool IsActive { get; init; }
}
