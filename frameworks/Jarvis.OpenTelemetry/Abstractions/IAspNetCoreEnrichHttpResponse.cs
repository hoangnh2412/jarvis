using System.Diagnostics;
using Microsoft.AspNetCore.Http;

namespace Jarvis.OpenTelemetry.Abstractions;

public interface IAspNetCoreEnrichHttpResponse
{
    Task EnrichAsync(Activity activity, HttpResponse httpResponse);
}
