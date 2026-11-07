using Platform.OrgStructure;

namespace Platform.Host.Entities;

public sealed class OrgUnitMember : IOrgUnitMemberEntity
{
    public Guid Id { get; set; }

    public Guid OrgId { get; set; }

    public Guid OrganizationalUnitId { get; set; }

    public Guid UserId { get; set; }

    public Guid JobPositionId { get; set; }

    public Guid JobGradeId { get; set; }

    public Guid? JobTitleId { get; set; }

    public bool IsActive { get; set; }
}
