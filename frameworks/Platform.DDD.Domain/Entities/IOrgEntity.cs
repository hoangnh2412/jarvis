namespace Platform.DDD.Domain.Entities;

/// <summary>
/// Marker: bản ghi persistence thuộc một organization, cùng vai trò với <see cref="ITenantEntity"/>.
/// <see cref="OrgId"/> là cột organization dùng cho data scope (RBAC+ABAC).
/// </summary>
public interface IOrgEntity
{
    /// <summary>Organization sở hữu bản ghi. Null khi bản ghi dùng chung trong tenant.</summary>
    Guid? OrgId { get; set; }
}
