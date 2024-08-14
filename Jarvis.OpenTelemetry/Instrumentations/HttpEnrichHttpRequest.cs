using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Jarvis.OpenTelemetry.Interfaces;
using Jarvis.OpenTelemetry.SemanticConvention;

namespace Jarvis.OpenTelemetry.Instrumentations;

public class HttpEnrichHttpRequest : IAspNetCoreEnrichHttpRequest
{
    public Task EnrichAsync(Activity activity, HttpRequest httpRequest)
    {
        foreach (var header in httpRequest.Headers)
        {
            activity.SetTag(string.Format(HttpAttributes.RequestHeader, header.Key), header.Value);
        }
        return Task.CompletedTask;
    }
}