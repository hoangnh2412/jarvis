using Platform.DDD.Domain.Entities;

namespace Platform.OrgStructure;

/// <summary>Vị trí trên sơ đồ. Chưa gán user.</summary>
public interface IPositionEntity : IEntity<Guid>
{
    Guid OrgId { get; set; }

    Guid OrganizationalUnitId { get; set; }

    Guid? JobTitleId { get; set; }

    Guid? ReportsToPositionId { get; set; }

    bool IsActive { get; set; }
}
