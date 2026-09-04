namespace Jarvis.Modules.Notifications.Redis;

public static class RedisNotificationDefaults
{
    public const string StoreConnectionServiceKey = "Notifications.Redis.Store";
    public const string DefaultStoreKeyPrefix = "signalr:notification";
    public const int DefaultRetentionDays = 20;
    public const int DefaultRedisRetryCount = 2;
}
