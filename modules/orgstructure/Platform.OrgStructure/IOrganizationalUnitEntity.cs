using Platform.DDD.Domain.Entities;

namespace Platform.OrgStructure;

/// <summary>Nút cây đơn vị trong một Org. Host sở hữu class concrete và migration.</summary>
public interface IOrganizationalUnitEntity : IEntity<Guid>
{
    Guid OrgId { get; set; }

    Guid TenantId { get; set; }

    Guid? ParentId { get; set; }

    string Code { get; set; }

    string Name { get; set; }

    string UnitType { get; set; }

    int SortOrder { get; set; }

    bool IsActive { get; set; }

    Guid? LegalEntityId { get; set; }

    string Path { get; set; }
}
