using Platform.DDD.Domain.Entities;

namespace Platform.Orgs;

/// <summary>
/// Cột chuẩn catalog Org trên Master DB. Host implement interface này và thêm property riêng.
/// </summary>
public interface IOrgManagementEntity : ITenantEntity, IEntity<Guid>
{
    Guid? ParentId { get; set; }

    string Code { get; set; }

    string Name { get; set; }

    bool IsActive { get; set; }
}
