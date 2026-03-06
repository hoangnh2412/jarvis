using System.Diagnostics;
using Jarvis.OpenTelemetry.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Jarvis.OpenTelemetry;

public class EnrichTraceMiddleware(
    RequestDelegate next,
    ILogger<EnrichTraceMiddleware> logger)
{
    private readonly RequestDelegate _next = next;
    private readonly ILogger<EnrichTraceMiddleware> _logger = logger;

    public async Task InvokeAsync(HttpContext context)
    {
        var traceIdHeader = context.Request.Headers.TraceParent.ToString();

        var states = new Dictionary<string, string>();
        var services = context.RequestServices.GetServices<IEnrichTraceService>();
        foreach (var service in services)
        {
            var items = await service.ExtractAsync();
            foreach (var item in items)
            {
                states.TryAdd(item.Key, item.Value);
            }
        }

        Activity? activity = null;

        if (Activity.Current != null)
        {
            if (!string.IsNullOrEmpty(traceIdHeader))
            {
                if (traceIdHeader.Split('-').Length == 4)
                {
                    try
                    {
                        var traceId = ActivityTraceId.CreateFromString(traceIdHeader.Split('-')[1]);
                        var spanId = ActivitySpanId.CreateRandom();

                        activity = new Activity("LinkedRequest");
                        if (activity != null)
                        {
                            activity.SetIdFormat(ActivityIdFormat.W3C);
                            activity.SetParentId(traceId, spanId, ActivityTraceFlags.Recorded);
                            activity.Start();
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Error parsing traceparent header");
                    }
                }
            }

            foreach (var item in states)
            {
                Activity.Current.SetTag(item.Key, item.Value);
            }
        }
        else
        {
            foreach (var item in states)
            {
                activity?.SetTag(item.Key, item.Value);
            }
        }

        await _next(context);

        activity?.Stop();
    }
}