using Microsoft.AspNetCore.SignalR;

namespace Platform.Realtime.SignalR.Hosting;

internal sealed class RealtimeSignalRBuilderHolder
{
    public ISignalRServerBuilder? ServerBuilder { get; set; }
}
