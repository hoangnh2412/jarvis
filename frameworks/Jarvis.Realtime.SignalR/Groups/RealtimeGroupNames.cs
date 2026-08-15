namespace Jarvis.Realtime.SignalR.Groups;

public static class RealtimeGroupNames
{
    public static string ForUser(Guid userId) => $"user:{userId:D}";

    public static string ForTenant(Guid tenantId) => $"tenant:{tenantId:D}";
}
