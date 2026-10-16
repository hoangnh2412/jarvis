namespace Platform.Realtime.Configuration;

public sealed class PlatformRealtimeOptions
{
    public const string SectionName = "Realtime";
    public string ClientMethodName { get; set; } = "notification";
    public RealtimeSignalROptions SignalR { get; set; } = new();
    public RealtimeSseOptions Sse { get; set; } = new();
    public RealtimeRedisOptions Redis { get; set; } = new();
}
