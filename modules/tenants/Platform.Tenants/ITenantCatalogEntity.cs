using Platform.DDD.Domain.Entities;

namespace Platform.Tenants;

/// <summary>
/// Bản ghi catalog tenant trên Master DB. Không phải <c>ITenantEntity</c>:
/// dấu đó gắn dòng nghiệp vụ thuộc một tenant. Host thêm cột riêng trên class concrete.
/// </summary>
public interface ITenantCatalogEntity : IEntity<Guid>
{
    string Code { get; set; }

    string Name { get; set; }

    bool IsActive { get; set; }

    Guid? ParentTenantId { get; set; }

    string ConnectionString { get; set; }
}
