using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using Platform.Realtime.Configuration;

namespace Platform.Realtime.Hosting;

/// <summary>
/// Bộ dựng fluent đăng ký transport realtime.
/// </summary>
public sealed class RealtimeBuilder
{
    internal RealtimeBuilder(
        IHostApplicationBuilder hostBuilder,
        PlatformRealtimeOptions options)
    {
        HostBuilder = hostBuilder;
        Options = options;
    }

    public IHostApplicationBuilder HostBuilder { get; }
    public PlatformRealtimeOptions Options { get; }
    internal ISignalRServerBuilder? SignalRServerBuilder { get; set; }
}
