namespace UnitTest.Infrastructure;

internal static class MultitenancyEfTestDatabaseNames
{
    public const string Master = "PlatformEfTests_Master";
    public const string TenantPlaceholder = "PlatformEfTests_Tenant_Placeholder";

    public static string Tenant(Guid tenantId) => $"PlatformEfTests_Tenant_{tenantId:N}";
}
