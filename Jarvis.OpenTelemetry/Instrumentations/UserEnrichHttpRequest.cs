using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Jarvis.OpenTelemetry.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Jarvis.OpenTelemetry.Instrumentations;

public class UserEnrichHttpRequest : IAspNetCoreEnrichHttpRequest
{
    public Task EnrichAsync(Activity activity, HttpRequest httpRequest)
    {
        var resolver = httpRequest.HttpContext.RequestServices.GetService<IUserInfoResolver>();
        if (resolver == null)
            return Task.CompletedTask;

        var info = resolver.Resolve(httpRequest.HttpContext);
        foreach (var item in info)
        {
            activity.SetTag(item.Key, item.Value);
        }

        return Task.CompletedTask;
    }
}