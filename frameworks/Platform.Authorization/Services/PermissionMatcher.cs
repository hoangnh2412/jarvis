namespace Platform.Authorization.Services;

internal static class PermissionMatcher
{
    public static bool IsWildcard(string permission) => permission is "*" or "System.All";

    public static bool HasWildcard(IEnumerable<string> granted) => granted.Any(IsWildcard);

    public static bool Grants(IEnumerable<string> granted, string required) =>
        granted.Any(g => IsWildcard(g) || g == required);
}
