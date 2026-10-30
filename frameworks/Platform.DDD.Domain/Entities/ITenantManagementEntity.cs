namespace Platform.DDD.Domain.Entities;

/// <summary>Registry tenant trên Master DB — connection string và phân cấp catalog.</summary>
public interface ITenantManagementEntity : IEntity<Guid>
{
    Guid? ParentId { get; set; }

    string ConnectionString { get; set; }
}
