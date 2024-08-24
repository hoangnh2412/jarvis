using Microsoft.AspNetCore.Http;

namespace Jarvis.OpenTelemetry.Interfaces;

public interface IUserInfoResolver
{
    /// <summary>
    /// Key is UserAttributes from Semantic convention
    /// </summary>
    /// <param name="httpContext"></param>
    /// <returns></returns>
    IDictionary<string, string> Resolve(HttpContext httpContext);
}