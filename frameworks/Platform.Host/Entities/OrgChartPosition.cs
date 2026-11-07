using Platform.OrgStructure;

namespace Platform.Host.Entities;

public sealed class OrgChartPosition : IPositionEntity
{
    public Guid Id { get; set; }

    public Guid OrgId { get; set; }

    public Guid OrganizationalUnitId { get; set; }

    public Guid? JobTitleId { get; set; }

    public Guid? ReportsToPositionId { get; set; }

    public bool IsActive { get; set; }
}
