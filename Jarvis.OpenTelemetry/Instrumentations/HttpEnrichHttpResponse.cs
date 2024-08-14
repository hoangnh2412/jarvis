using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Jarvis.OpenTelemetry.Interfaces;
using Jarvis.OpenTelemetry.SemanticConvention;

namespace Jarvis.OpenTelemetry.Instrumentations;

public class HttpEnrichHttpResponse : IAspNetCoreEnrichHttpResponse
{
    public Task EnrichAsync(Activity activity, HttpResponse httpResponse)
    {
        foreach (var header in httpResponse.Headers)
        {
            activity.SetTag(string.Format(HttpAttributes.ResponseHeader, header.Key), header.Value);
        }
        return Task.CompletedTask;
    }
}