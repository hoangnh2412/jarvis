namespace Platform.Orgs;

/// <summary>Giới hạn cột chuẩn — EF configuration và manager dùng chung.</summary>
public static class OrgCatalogLimits
{
    public const int CodeMaxLength = 64;

    public const int NameMaxLength = 256;

    /// <summary>Trim + lowercase invariant — gọi trước khi query/persist (không transform cột trên SQL).</summary>
    public static string NormalizeCode(string code) =>
        code.Trim().ToLowerInvariant();
}
