using System.Security.Claims;

namespace Jarvis.Shared.Extensions;

/// <summary>
/// Provides extension functions for ClaimsPrincipal
/// </summary>
public static partial class ClaimsPrincipalExtension
{
    /// <summary>
    /// Check the claim exists in the claim list
    /// </summary>
    /// <param name="principal">Instance ClaimPrincipal</param>
    /// <param name="match">Delegate validate claim</param>
    /// <returns></returns>
    public static bool HasClaim(this ClaimsPrincipal principal, Predicate<Claim> match)
    {
        return principal.Claims.Any(x => match(x));
    }

    /// <summary>
    /// Filter claim by name in the claim list
    /// Note: Search both the list and the claim properties
    /// </summary>
    /// <param name="claims">List of claim to filter</param>
    /// <param name="name">The name of claim to filter</param>
    /// <returns></returns>
    public static Claim GetClaim(IEnumerable<Claim> claims, string name)
    {
        var claim = claims.FirstOrDefault(x => x.Type == name);
        if (claim != null)
            return claim;

        claim = claims
            .Where(x => x.Properties != null)
            .FirstOrDefault(x => x.Properties.Values.Contains(name));

        if (claim != null)
            return claim;

        return null;
    }

    public static T GetClaim<T>(ClaimsPrincipal principal, string claimType)
    {
        var claim = ClaimsPrincipalExtension.GetClaim(principal.Claims, claimType);
        if (claim == null || claim.Value == null)
            return default(T);

        return (T)Convert.ChangeType(claim.Value, typeof(T));
    }
}