using Microsoft.AspNetCore.Http;

namespace Jarvis.OpenTelemetry.Abstractions;

public interface IUserInfoResolver
{
    /// <summary>
    /// Keys should follow semantic conventions (e.g. <c>enduser.*</c>).
    /// </summary>
    IDictionary<string, string> Resolve(HttpContext httpContext);
}
