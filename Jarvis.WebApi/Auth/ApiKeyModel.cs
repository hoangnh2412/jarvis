using System.Security.Claims;
using AspNetCore.Authentication.ApiKey;

namespace Jarvis.WebApi.Auth;

public class ApiKeyModel : IApiKey
{
    public ApiKeyModel(string key, string owner, List<Claim> claims = null)
    {
        Key = key;
        OwnerName = owner;
        Claims = claims ?? new List<Claim>();
    }

    public string Key { get; }
    public string OwnerName { get; }
    public IReadOnlyCollection<Claim> Claims { get; }
}