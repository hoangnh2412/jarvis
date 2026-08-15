using System.Diagnostics;
using Jarvis.OpenTelemetry.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Jarvis.OpenTelemetry.Middleware;

/// <summary>
/// Applies <see cref="IEnrichTraceService"/> data to <see cref="Activity.Current"/>.
/// Incoming W3C trace context is handled by ASP.NET Core + OpenTelemetry instrumentation; this middleware only adds tags.
/// </summary>
public sealed class TraceEnrichmentMiddleware(RequestDelegate next)
{
    private readonly RequestDelegate _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        var states = new Dictionary<string, string>();
        foreach (var service in context.RequestServices.GetServices<IEnrichTraceService>())
        {
            foreach (var item in await service.ExtractAsync())
                states.TryAdd(item.Key, item.Value);
        }

        var activity = Activity.Current;
        if (activity != null)
        {
            foreach (var item in states)
                activity.SetTag(item.Key, item.Value);
        }

        await _next(context);
    }
}
