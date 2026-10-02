using System.Diagnostics;
using Microsoft.AspNetCore.Http;

namespace Platform.OpenTelemetry.Abstractions;

public interface IAspNetCoreEnrichHttpResponse
{
    Task EnrichAsync(Activity activity, HttpResponse httpResponse);
}
