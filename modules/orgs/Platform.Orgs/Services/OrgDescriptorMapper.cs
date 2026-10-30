using Platform.Orgs.Abstractions;

namespace Platform.Orgs.Services;

internal static class OrgDescriptorMapper
{
    public static OrgDescriptor ToDescriptor<TOrg>(TOrg entity)
        where TOrg : class, IOrgManagementEntity
    {
        return new OrgDescriptor
        {
            Id = entity.Id,
            TenantId = entity.TenantId,
            Code = entity.Code,
            Name = entity.Name,
            IsActive = entity.IsActive,
            ParentId = entity.ParentId,
        };
    }
}
