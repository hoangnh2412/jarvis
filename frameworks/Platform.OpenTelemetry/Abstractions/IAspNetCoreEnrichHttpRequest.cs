using System.Diagnostics;
using Microsoft.AspNetCore.Http;

namespace Platform.OpenTelemetry.Abstractions;

public interface IAspNetCoreEnrichHttpRequest
{
    Task EnrichAsync(Activity activity, HttpRequest httpRequest);
}
