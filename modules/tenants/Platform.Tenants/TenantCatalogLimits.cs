namespace Platform.Tenants;

/// <summary>Giới hạn cột chuẩn catalog tenant.</summary>
public static class TenantCatalogLimits
{
    public const int CodeMaxLength = 64;

    public const int NameMaxLength = 256;

    public const int ConnectionStringMaxLength = 2048;

    public static string NormalizeCode(string code) =>
        code.Trim().ToLowerInvariant();
}
