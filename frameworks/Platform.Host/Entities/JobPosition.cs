using Platform.OrgStructure;

namespace Platform.Host.Entities;

public sealed class JobPosition : IOrgNamedCatalogEntity
{
    public Guid Id { get; set; }

    public Guid OrgId { get; set; }

    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public int SortOrder { get; set; }
}
