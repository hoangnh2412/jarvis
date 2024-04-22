using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Jarvis.Application.Interfaces;
using Jarvis.Shared.Auth;
using Jarvis.Shared.Extensions;
using System.Security.Claims;

namespace UnitTest;
public class TestWorkContext : IWorkContext
{
    private UserInfoModel UserInfo { get; }

    public Guid TokenId => FindClaim<Guid>(ClaimTypes.SerialNumber);

    public Guid TenantId => FindClaim<Guid>(ClaimTypes.GroupSid);

    public Guid UserId => UserInfo.Id;

    public string UserName => UserInfo.UserName;

    public string FirstName => UserInfo.FirstName;

    public string LastName => UserInfo.LastName;

    public string FullName => UserInfo.FullName;

    UserInfoModel IWorkContext.UserInfo => GetUserInfor();

    private readonly IHttpContextAccessor _httpContextAccessor;

    public TestWorkContext(
        IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;

        UserInfo = GetUserInfor();
    }

    public bool IsInRole(string roleName)
    {
        return _httpContextAccessor.HttpContext.User.Claims.Any(x => x.Type == ClaimTypes.Role && x.Value == roleName);
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

    private T FindClaim<T>(string claimType)
    {
        return ClaimsPrincipalExtension.GetClaim<T>(_httpContextAccessor.HttpContext.User, claimType);
    }
}