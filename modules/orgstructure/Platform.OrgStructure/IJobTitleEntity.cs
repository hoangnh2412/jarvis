using Platform.DDD.Domain.Entities;

namespace Platform.OrgStructure;

/// <summary>Chức danh trong một Org.</summary>
public interface IJobTitleEntity : IEntity<Guid>
{
    Guid OrgId { get; set; }

    string Code { get; set; }

    string Name { get; set; }
}
