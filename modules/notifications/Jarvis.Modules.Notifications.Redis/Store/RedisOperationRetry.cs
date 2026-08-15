using StackExchange.Redis;

namespace Jarvis.Modules.Notifications.Redis.Store;

internal static class RedisOperationRetry
{
    internal static async Task ExecuteAsync(
        Func<CancellationToken, Task> operation,
        CancellationToken cancellationToken,
        int maxRetries = RedisNotificationDefaults.DefaultRedisRetryCount)
    {
        var attempt = 0;
        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                await operation(cancellationToken).ConfigureAwait(false);
                return;
            }
            catch (Exception ex) when (IsTransient(ex) && attempt < maxRetries)
            {
                attempt++;
                await Task.Delay(TimeSpan.FromMilliseconds(100 * attempt), cancellationToken)
                    .ConfigureAwait(false);
            }
        }
    }

    private static bool IsTransient(Exception ex) =>
        ex is RedisTimeoutException or RedisConnectionException;
}
