using Microsoft.Extensions.DependencyInjection;
using Platform.Realtime.Extensions;
using Platform.Realtime.Hosting;
using Platform.Realtime.Sse.Services;

namespace Platform.Realtime.Sse.Extensions;

public static class RealtimeSseRedisFanOutExtensions
{
    /// <summary>
    /// Bật Redis pub/sub fan-out cho SSE scale-out (shared multiplexer từ core).
    /// </summary>
    public static RealtimeBuilder UseSseRedisFanOut(this RealtimeBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        builder.EnsureRealtimeRedis();

        return builder;
    }
}
