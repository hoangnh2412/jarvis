namespace Platform.Authentication.Identity;

/// <summary>
/// Claim types shared by Identity user/role administration.
/// Permission values stay role claims; this type is not a permission catalog.
/// </summary>
public static class IdentityClaimTypes
{
    public const string DisplayName = "display_name";

    public const string Permission = "Permission";

    /// <summary>Absent or any value other than <c>false</c> means the role is active.</summary>
    public const string RoleActive = "role_active";
}
