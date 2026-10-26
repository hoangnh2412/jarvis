using Platform.DDD.Domain.Entities;

namespace Platform.OrgStructure;

/// <summary>Pháp nhân trong một Org. Đơn vị gắn qua <c>LegalEntityId</c>.</summary>
public interface ILegalEntityEntity : IEntity<Guid>
{
    Guid OrgId { get; set; }

    string Code { get; set; }

    string Name { get; set; }

    string? TaxId { get; set; }

    bool IsActive { get; set; }
}
