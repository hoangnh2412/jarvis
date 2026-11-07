using Platform.OrgStructure;

namespace Platform.Host.Entities;

public sealed class LegalEntity : ILegalEntityProfileEntity
{
    public Guid Id { get; set; }

    public Guid OrgId { get; set; }

    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? TaxId { get; set; }

    public bool IsActive { get; set; }

    public string? DetailsJson { get; set; }
}
