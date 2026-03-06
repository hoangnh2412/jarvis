using Jarvis.OpenTelemetry.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Jarvis.OpenTelemetry;

public class EnrichLogMiddleware(
    RequestDelegate next,
    ILogger<EnrichLogMiddleware> logger)
{
    private readonly RequestDelegate _next = next;
    private readonly ILogger<EnrichLogMiddleware> _logger = logger;

    public async Task InvokeAsync(HttpContext context)
    {
        var states = new Dictionary<string, string>();
        var services = context.RequestServices.GetServices<IEnrichLogService>();
        foreach (var service in services)
        {
            var items = await service.ExtractAsync();
            foreach (var item in items)
            {
                states.TryAdd(item.Key, item.Value);
            }
        }

        using (_logger.BeginScope(states))
        {
            await _next(context);
        }
    }
}