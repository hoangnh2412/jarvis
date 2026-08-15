namespace Jarvis.Modules.Notifications.Redis.Store;

internal static class RedisNotificationStoreScripts
{
/// <summary>
/// Lưu atomic: SET item + ZADD index + ZADD unread + ZREM read.
/// KEYS: item, index, unread, read — ARGV: json, member, score, ttlMs.
/// </summary>
    internal const string SaveNotification = """
        redis.call('SET', KEYS[1], ARGV[1], 'PX', ARGV[4])
        redis.call('ZADD', KEYS[2], ARGV[3], ARGV[2])
        redis.call('ZADD', KEYS[3], ARGV[3], ARGV[2])
        redis.call('ZREM', KEYS[4], ARGV[2])
        return 1
        """;
}
