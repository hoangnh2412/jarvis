using System.Diagnostics;
using Jarvis.OpenTelemetry.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Jarvis.OpenTelemetry.Instrumentations;

public sealed class UserRequestEnrichment : IAspNetCoreEnrichHttpRequest
{
    public Task EnrichAsync(Activity activity, HttpRequest httpRequest)
    {
        var resolver = httpRequest.HttpContext.RequestServices.GetService<IUserInfoResolver>();
        if (resolver == null)
            return Task.CompletedTask;

        foreach (var item in resolver.Resolve(httpRequest.HttpContext))
            activity.SetTag(item.Key, item.Value);

        return Task.CompletedTask;
    }
}
