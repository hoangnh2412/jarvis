using System.Diagnostics;
using Microsoft.AspNetCore.Http;

namespace Jarvis.OpenTelemetry.Interfaces;

public interface IAspNetCoreEnrichHttpRequest
{
    Task EnrichAsync(Activity activity, HttpRequest httpRequest);
}