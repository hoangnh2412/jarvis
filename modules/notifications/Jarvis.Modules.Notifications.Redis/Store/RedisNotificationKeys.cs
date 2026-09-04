namespace Jarvis.Modules.Notifications.Redis.Store;

/// <summary>
/// Hash tag <c>{tenantId:userId:notifications}</c> để giữ các key liên quan cùng slot Redis Cluster.
/// </summary>
public static class RedisNotificationKeys
{
    public static string Scope(Guid tenantId, Guid userId) =>
        $"{{{tenantId:D}:{userId:D}:notifications}}";

    public static string Item(string keyPrefix, Guid tenantId, Guid userId, Guid notificationId) =>
        $"{keyPrefix}:{Scope(tenantId, userId)}:item:{notificationId:D}";

    public static string Index(string keyPrefix, Guid tenantId, Guid userId) =>
        $"{keyPrefix}:{Scope(tenantId, userId)}:index";

    public static string Unread(string keyPrefix, Guid tenantId, Guid userId) =>
        $"{keyPrefix}:{Scope(tenantId, userId)}:unread";

    public static string Read(string keyPrefix, Guid tenantId, Guid userId) =>
        $"{keyPrefix}:{Scope(tenantId, userId)}:read";
}
