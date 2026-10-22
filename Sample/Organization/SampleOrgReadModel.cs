using Platform.Orgs.Mapping;

namespace Sample.Organization;

/// <summary>Read model admin Sample — gồm cột host.</summary>
public sealed class SampleOrgDetail
{
    public Guid Id { get; init; }

    public Guid TenantId { get; init; }

    public string Code { get; init; } = string.Empty;

    public string Name { get; init; } = string.Empty;

    public bool IsActive { get; init; }

    public string? TaxCode { get; init; }
}

public sealed class SampleOrgReadMapper : IOrgCatalogReadMapper<Sample.Entities.Org, SampleOrgDetail>
{
    public SampleOrgDetail Map(Sample.Entities.Org entity) =>
        new()
        {
            Id = entity.Id,
            TenantId = entity.TenantId,
            Code = entity.Code,
            Name = entity.Name,
            IsActive = entity.IsActive,
            TaxCode = entity.TaxCode,
        };
}
