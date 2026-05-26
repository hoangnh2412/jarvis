using Jarvis.OpenTelemetry.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Jarvis.OpenTelemetry.Middleware;

/// <summary>
/// Pushes <see cref="IEnrichLogService"/> key/value pairs into a logging scope for the request.
/// </summary>
public sealed class LogEnrichmentMiddleware(
    RequestDelegate next,
    ILogger<LogEnrichmentMiddleware> logger)
{
    private readonly RequestDelegate _next = next;
    private readonly ILogger<LogEnrichmentMiddleware> _logger = logger;

    public async Task InvokeAsync(HttpContext context)
    {
        var states = new Dictionary<string, string>();
        foreach (var service in context.RequestServices.GetServices<IEnrichLogService>())
        {
            foreach (var item in await service.ExtractAsync())
                states.TryAdd(item.Key, item.Value);
        }

        using (_logger.BeginScope(states))
            await _next(context);
    }
}
