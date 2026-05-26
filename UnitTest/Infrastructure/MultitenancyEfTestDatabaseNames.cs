namespace UnitTest.Infrastructure;

internal static class MultitenancyEfTestDatabaseNames
{
    public const string Master = "JarvisEfTests_Master";
    public const string TenantPlaceholder = "JarvisEfTests_Tenant_Placeholder";

    public static string Tenant(Guid tenantId) => $"JarvisEfTests_Tenant_{tenantId:N}";
}
