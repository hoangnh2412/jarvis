using System.Diagnostics;
using Microsoft.AspNetCore.Http;

namespace Jarvis.OpenTelemetry.Abstractions;

public interface IAspNetCoreEnrichHttpRequest
{
    Task EnrichAsync(Activity activity, HttpRequest httpRequest);
}
