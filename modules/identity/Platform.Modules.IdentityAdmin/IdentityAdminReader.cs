namespace Platform.Modules.IdentityAdmin;

using System.Security.Claims;
using Platform.Authentication.Identity;

internal static class IdentityAdminReader
{
    public static string? DisplayName(IEnumerable<Claim> claims) =>
        claims.FirstOrDefault(claim => claim.Type == IdentityClaimTypes.DisplayName)?.Value;

    public static bool IsActive(IEnumerable<Claim> claims)
    {
        var value = claims.FirstOrDefault(claim => claim.Type == IdentityClaimTypes.RoleActive)?.Value;
        return !string.Equals(value, "false", StringComparison.OrdinalIgnoreCase);
    }

    public static IReadOnlyList<string> Permissions(IEnumerable<Claim> claims) =>
        claims
            .Where(claim => claim.Type == IdentityClaimTypes.Permission)
            .Select(claim => claim.Value)
            .Distinct(StringComparer.Ordinal)
            .OrderBy(value => value, StringComparer.Ordinal)
            .ToList();
}
