using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Jarvis.OpenTelemetry.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Jarvis.OpenTelemetry.Instrumentations;

public class UserEnrichHttpResponse : IAspNetCoreEnrichHttpResponse
{
    public Task EnrichAsync(Activity activity, HttpResponse httpResponse)
    {
        var resolver = httpResponse.HttpContext.RequestServices.GetService<IUserInfoResolver>();
        if (resolver == null)
            return Task.CompletedTask;

        var info = resolver.Resolve(httpResponse.HttpContext);
        foreach (var item in info)
        {
            activity.SetTag(item.Key, item.Value);
        }

        return Task.CompletedTask;
    }
}