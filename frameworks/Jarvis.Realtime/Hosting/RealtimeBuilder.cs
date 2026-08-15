using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using Jarvis.Realtime.Configuration;

namespace Jarvis.Realtime.Hosting;

/// <summary>
/// Bộ dựng fluent đăng ký transport realtime.
/// </summary>
public sealed class RealtimeBuilder
{
    internal RealtimeBuilder(
        IHostApplicationBuilder hostBuilder,
        JarvisRealtimeOptions options)
    {
        HostBuilder = hostBuilder;
        Options = options;
    }

    public IHostApplicationBuilder HostBuilder { get; }
    public JarvisRealtimeOptions Options { get; }
    internal ISignalRServerBuilder? SignalRServerBuilder { get; set; }
}
