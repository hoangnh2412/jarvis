using Microsoft.AspNetCore.Http;
using Jarvis.Application.Interfaces;
using Jarvis.Shared.Auth;
using Jarvis.Shared.Extensions;
using System.Security.Claims;
using Newtonsoft.Json;

namespace Jarvis.WebApi;

public class WorkContext : IWorkContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public Guid TokenId => FindClaim<Guid>(ClaimTypes.SerialNumber);

    public Guid TenantId => FindClaim<Guid>(ClaimTypes.GroupSid);

    public Guid UserId => FindClaim<Guid>(ClaimTypes.Sid);

    public string UserName => FindClaim<string>(ClaimTypes.NameIdentifier);

    public string FirstName => FindClaim<string>(ClaimTypes.Surname);

    public string LastName => FindClaim<string>(ClaimTypes.GivenName);

    public string FullName => FindClaim<string>(ClaimTypes.Name);

    public UserInfoModel UserInfo
    {
        get
        {
            var data = FindClaim<string>(ClaimTypes.UserData);
            if (string.IsNullOrEmpty(data))
                return null;

            return JsonConvert.DeserializeObject<UserInfoModel>(data);
        }
    }

    public WorkContext(
        IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private T FindClaim<T>(string claimType)
    {
        return ClaimsPrincipalExtension.GetClaim<T>(_httpContextAccessor.HttpContext.User, claimType);
    }

    public bool IsInRole(string roleName)
    {
        return _httpContextAccessor.HttpContext.User.Claims.Any(x => x.Type == ClaimTypes.Role && x.Value == roleName);
    }
}