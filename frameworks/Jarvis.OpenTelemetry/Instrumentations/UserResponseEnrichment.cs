using System.Diagnostics;
using Jarvis.OpenTelemetry.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Jarvis.OpenTelemetry.Instrumentations;

public sealed class UserResponseEnrichment : IAspNetCoreEnrichHttpResponse
{
    public Task EnrichAsync(Activity activity, HttpResponse httpResponse)
    {
        var resolver = httpResponse.HttpContext.RequestServices.GetService<IUserInfoResolver>();
        if (resolver == null)
            return Task.CompletedTask;

        foreach (var item in resolver.Resolve(httpResponse.HttpContext))
            activity.SetTag(item.Key, item.Value);

        return Task.CompletedTask;
    }
}
