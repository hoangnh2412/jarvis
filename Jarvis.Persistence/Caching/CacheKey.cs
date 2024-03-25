namespace Jarvis.Persistence.Caching;

public static partial class CacheKey
{
    public static string Setting = "Setting";
    public static string Tenant = "Tenant";

    public static string Build(Guid tenantCode, string cacheKey)
    {
        return $"{cacheKey}:{tenantCode}";
    }
}