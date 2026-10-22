using Platform.Orgs.Abstractions;

namespace Platform.Orgs.Services;

internal static class OrgDescriptorMapper
{
    public static OrgDescriptor ToDescriptor<TOrg>(TOrg entity)
        where TOrg : class, IOrgCatalogEntity
    {
        return new OrgDescriptor
        {
            Id = entity.Id,
            TenantId = entity.TenantId,
            Code = entity.Code,
            Name = entity.Name,
            IsActive = entity.IsActive,
        };
    }
}
