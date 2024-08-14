using System.Diagnostics;
using Microsoft.AspNetCore.Http;

namespace Jarvis.OpenTelemetry.Interfaces;

public interface IAspNetCoreEnrichHttpResponse
{
    Task EnrichAsync(Activity activity, HttpResponse httpResponse);
}