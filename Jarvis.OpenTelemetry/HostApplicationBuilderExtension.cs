using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;

namespace Jarvis.OpenTelemetry;

public static class HostApplicationBuilderExtension
{
    public static IApplicationBuilder UseOTEL(this IApplicationBuilder builder)
    {
        builder.UseMiddleware<EnrichTraceMiddleware>();
        builder.UseMiddleware<EnrichLogMiddleware>();
        return builder;
    }
}