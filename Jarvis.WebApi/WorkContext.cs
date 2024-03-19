using Microsoft.AspNetCore.Http;
using Jarvis.Application.Interfaces;
using Jarvis.Shared.Auth;
using Jarvis.Shared.Extensions;

namespace Jarvis.WebApi;
public class WorkContext : IWorkContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public WorkContext(
        IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid GetTokenKey()
    {
        // var claim = ClaimsPrincipalExtension.GetClaim(_httpContextAccessor.HttpContext.User.Claims, JwtRegisteredClaimNames.Jti);
        var claim = ClaimsPrincipalExtension.GetClaim(_httpContextAccessor.HttpContext.User.Claims, "jti");
        if (claim == null || claim.Value == null)
            return Guid.Empty;

        return Guid.Parse(claim.Value);
    }

    public Guid GetTenantKey()
    {
        // var claim = ClaimsPrincipalExtension.GetClaim(_httpContextAccessor.HttpContext.User.Claims, JwtRegisteredClaimNames.Sub);
        var claim = ClaimsPrincipalExtension.GetClaim(_httpContextAccessor.HttpContext.User.Claims, "sub");
        if (claim == null || claim.Value == null)
            return Guid.Empty;

        return Guid.Parse(claim.Value);
    }

    public Guid GetUserKey()
    {
        // var claim = ClaimsPrincipalExtension.GetClaim(_httpContextAccessor.HttpContext.User.Claims, JwtRegisteredClaimNames.Sid);
        var claim = ClaimsPrincipalExtension.GetClaim(_httpContextAccessor.HttpContext.User.Claims, "sid");
        if (claim == null || claim.Value == null)
            return Guid.Empty;

        return Guid.Parse(claim.Value);
    }

    public string GetUserName()
    {
        // var claim = ClaimsPrincipalExtension.GetClaim(_httpContextAccessor.HttpContext.User.Claims, JwtRegisteredClaimNames.NameId);
        var claim = ClaimsPrincipalExtension.GetClaim(_httpContextAccessor.HttpContext.User.Claims, "nameId");
        if (claim == null || claim.Value == null)
            return null;

        return claim.Value;
    }

    public string GetUserFullName()
    {
        // var claim = ClaimsPrincipalExtension.GetClaim(_httpContextAccessor.HttpContext.User.Claims, JwtRegisteredClaimNames.Name);
        var claim = ClaimsPrincipalExtension.GetClaim(_httpContextAccessor.HttpContext.User.Claims, "name");
        if (claim == null || claim.Value == null)
            return null;

        return claim.Value;
    }

    public UserInfoModel GetUserInfo()
    {
        throw new NotImplementedException();
    }
}