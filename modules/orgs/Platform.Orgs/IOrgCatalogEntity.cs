using Platform.DDD.Domain.Entities;

namespace Platform.Orgs;

/// <summary>
/// Cột chuẩn của catalog Org. Host implement interface này và thêm property riêng.
/// Package không chứa class entity concrete.
/// </summary>
public interface IOrgCatalogEntity : ITenantEntity, IEntity<Guid>
{
    string Code { get; set; }

    string Name { get; set; }

    bool IsActive { get; set; }
}
