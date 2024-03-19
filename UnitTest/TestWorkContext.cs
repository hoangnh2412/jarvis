using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Jarvis.Application.Interfaces;
using Jarvis.Shared.Auth;
using Jarvis.Shared.Extensions;

namespace UnitTest;
public class TestWorkContext : IWorkContext
{
    private UserInfoModel UserInfo { get; }
    private readonly IHttpContextAccessor _httpContextAccessor;

    public TestWorkContext(
        IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;

        UserInfo = GetUserInfor();
    }

    private UserInfoModel GetUserInfor()
    {
        var token = _httpContextAccessor.HttpContext.Request.Headers["Authorization"].ToString();
        if (string.IsNullOrEmpty(token))
            return null;

        var accessToken = token.Substring(7);
        var jwtSecurityToken = new JwtSecurityToken(accessToken);
        var claims = jwtSecurityToken.Claims.ToList();

        var claim = claims.FirstOrDefault(x => x.Type == "UserInfo");
        if (claim == null)
            return null;

        var result = JsonConvert.DeserializeObject<UserInfoModel>(claim.Value);

        var emailVerified = ClaimsPrincipalExtension.GetClaim(_httpContextAccessor.HttpContext.User.Claims, "email_verified");
        if (emailVerified != null)
            result.EmailVerified = emailVerified.Value == "true";

        var phoneVerified = ClaimsPrincipalExtension.GetClaim(_httpContextAccessor.HttpContext.User.Claims, "phone_number_verified");
        if (phoneVerified != null)
            result.PhoneNumberVerified = phoneVerified.Value == "true";

        if (result?.OrgRoleList != null && result.OrgRoleList.Count > 0)
            result.Organization = JsonConvert.SerializeObject(result.OrgRoleList.Select(x => x.Org).ToArray());

        return result;
    }

    private UserInfoModel GetUserInforFromHeader()
    {
        var token = _httpContextAccessor.HttpContext.Request.Headers["Authorization"].ToString();
        if (string.IsNullOrEmpty(token))
            return null;

        var accessToken = token.Substring(7);

        // trim 'Bearer ' from the start since its just a prefix for the token string
        var jwtSecurityToken = new JwtSecurityToken(accessToken);
        var claims = jwtSecurityToken.Claims.ToList();

        var userCognito = new UserInfoModel()
        {
            AccessToken = accessToken
        };
        var phoneNumber = string.Empty;
        var email = string.Empty;
        foreach (var item in claims)
        {
            if (item.Type == "username" || item.Type == "cognito:username")
                userCognito.UserName = item.Value;

            if (item.Type == "email")
                email = item.Value;

            if (item.Type == "email_verified")
                userCognito.EmailVerified = item.Value == "true";

            if (item.Type == "phone_number")
                phoneNumber = item.Value;

            if (item.Type == "phone_number_verified")
                userCognito.PhoneNumberVerified = item.Value == "true";

            if (item.Type == "UserInfo")
            {
                UserInfoModel userInfo = JsonConvert.DeserializeObject<UserInfoModel>(item.Value);
                userCognito.FirstName = userInfo?.FirstName;
                userCognito.LastName = userInfo?.LastName;
                userCognito.Email = userInfo?.Email;
                userCognito.PhoneNumber = userInfo?.PhoneNumber;
                if (userInfo?.OrgRoleList != null && userInfo.OrgRoleList.Count > 0)
                {
                    userCognito.Organization = JsonConvert.SerializeObject(userInfo.OrgRoleList.Select(x => x.Org).ToArray());
                }
            }
        }

        if (!string.IsNullOrEmpty(userCognito.Email))
            userCognito.Email = email;

        if (!string.IsNullOrEmpty(userCognito.PhoneNumber))
            userCognito.PhoneNumber = phoneNumber;

        userCognito.Id = Guid.Parse(jwtSecurityToken.Subject);
        return userCognito;
    }

    public Guid GetTokenKey()
    {
        var claim = ClaimsPrincipalExtension.GetClaim(_httpContextAccessor.HttpContext.User.Claims, "jti");
        if (claim == null || claim.Value == null)
            return Guid.Empty;

        return Guid.Parse(claim.Value);
    }

    public Guid GetTenantKey()
    {
        return Guid.Empty;
        // var claim = ClaimsPrincipalExtension.GetClaim(_httpContextAccessor.HttpContext.User.Claims, JwtRegisteredClaimNames.Sub);
        // var claim = ClaimsPrincipalExtension.GetClaim(_httpContextAccessor.HttpContext.User.Claims, "sub");
        // if (claim == null || claim.Value == null)
        //     return Guid.Empty;

        // return Guid.Parse(claim.Value);
    }

    public Guid GetUserKey()
    {
        return UserInfo.Id;
    }

    public string GetUserName()
    {
        return UserInfo.UserName;
    }

    public string GetUserFullName()
    {
        return UserInfo.FullName;
    }

    public UserInfoModel GetUserInfo()
    {
        return UserInfo;
    }
}