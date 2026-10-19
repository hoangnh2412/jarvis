using Sample.Multitenancy;

namespace Sample.Authentication;

/// <summary>Tenant mặc định cho seed credential auth trong Sample.</summary>
internal static class SampleAuthTenants
{
    public static readonly Guid DevTenantId = MultitenancyEfJobRunner.MasterTenantRegistryId;
}
