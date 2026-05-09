using Microsoft.AspNetCore.Http;

namespace Jarvis.OpenTelemetry.Instrumentations;

internal static class AspNetCoreTracePathFilter
{
    /// <summary>Returns true when the request should be traced (not excluded).</summary>
    public static bool ShouldInstrument(HttpContext httpContext, IList<string> excludedPathPrefixes)
    {
        if (excludedPathPrefixes.Count == 0)
            return true;

        var path = httpContext.Request.Path.Value ?? string.Empty;
        if (path.Length == 0)
            path = "/";

        foreach (var prefix in excludedPathPrefixes)
        {
            var normalized = NormalizePrefix(prefix);
            if (normalized.Length == 0)
                continue;

            if (path.StartsWith(normalized, StringComparison.OrdinalIgnoreCase))
                return false;
        }

        return true;
    }

    private static string NormalizePrefix(string prefix)
    {
        var p = prefix.Trim();
        if (p.Length == 0)
            return string.Empty;
        return p.StartsWith('/') ? p : "/" + p;
    }
}
