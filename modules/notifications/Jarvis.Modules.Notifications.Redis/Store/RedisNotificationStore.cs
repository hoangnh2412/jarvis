using Microsoft.Extensions.Options;
using Jarvis.Modules.Notifications.Configuration;
using Jarvis.Modules.Notifications.Contracts;
using Jarvis.Modules.Notifications.Definitions;
using StackExchange.Redis;

namespace Jarvis.Modules.Notifications.Redis.Store;

/// <summary>
/// Redis notification store — TTL trên item + heal orphan lúc List; trạng thái đọc = membership ZSET.
/// </summary>
public sealed class RedisNotificationStore(
    IConnectionMultiplexer multiplexer,
    IOptions<NotificationInboxOptions> options,
    INotificationInboxSettings inboxSettings) : INotificationStore
{
    private readonly IDatabase _db = multiplexer.GetDatabase();
    private readonly NotificationInboxOptions _inbox = options.Value;

    private string KeyPrefix =>
        string.IsNullOrWhiteSpace(_inbox.InstanceName)
            ? RedisNotificationDefaults.DefaultStoreKeyPrefix
            : _inbox.InstanceName;

    public async Task SaveAsync(
        Guid tenantId,
        Guid userId,
        SignalRNotificationMessage message,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);
        cancellationToken.ThrowIfCancellationRequested();

        var retentionDays = await inboxSettings.GetRetentionDaysAsync(cancellationToken).ConfigureAwait(false);
        if (retentionDays <= 0)
            retentionDays = NotificationInboxSettingDefinition.DefaultRetentionDays;

        var redisRetryCount = await inboxSettings.GetRedisRetryCountAsync(cancellationToken).ConfigureAwait(false);
        if (redisRetryCount < 0)
            redisRetryCount = NotificationInboxSettingDefinition.DefaultRedisRetryCount;

        var created = message.CreatedAtUtc == default ? DateTimeOffset.UtcNow : message.CreatedAtUtc;
        var id = message.NotificationId == Guid.Empty ? Guid.CreateVersion7() : message.NotificationId;
        var expires = created.Add(TimeSpan.FromDays(retentionDays));
        var score = created.ToUnixTimeMilliseconds();

        var item = new SignalRNotificationItemDto
        {
            NotificationId = id,
            Type = message.Type,
            Title = message.Title,
            Body = message.Body,
            Data = message.Data,
            IsRead = false,
            CreatedAtUtc = created
        };

        var itemKey = RedisNotificationKeys.Item(KeyPrefix, tenantId, userId, id);
        var indexKey = RedisNotificationKeys.Index(KeyPrefix, tenantId, userId);
        var unreadKey = RedisNotificationKeys.Unread(KeyPrefix, tenantId, userId);
        var readKey = RedisNotificationKeys.Read(KeyPrefix, tenantId, userId);
        var json = NotificationStoreJson.Serialize(item);

        var itemTtl = RemainingTtl(expires);
        var member = id.ToString("D");
        var ttlMs = (long)itemTtl.TotalMilliseconds;

        await RedisOperationRetry.ExecuteAsync(
            async ct =>
            {
                await _db.ScriptEvaluateAsync(
                    RedisNotificationStoreScripts.SaveNotification,
                    [itemKey, indexKey, unreadKey, readKey],
                    [json, member, score, ttlMs]).ConfigureAwait(false);
            },
            cancellationToken,
            redisRetryCount).ConfigureAwait(false);
    }

    public async Task<SignalRNotificationItemDto?> GetAsync(
        Guid tenantId,
        Guid userId,
        Guid notificationId,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var key = RedisNotificationKeys.Item(KeyPrefix, tenantId, userId, notificationId);
        var unreadKey = RedisNotificationKeys.Unread(KeyPrefix, tenantId, userId);
        var value = await _db.StringGetAsync(key).ConfigureAwait(false);
        if (!value.HasValue)
            return null;

        var member = notificationId.ToString("D");
        var inUnread = await _db.SortedSetScoreAsync(unreadKey, member).ConfigureAwait(false);
        return NotificationStoreJson.Deserialize(value!, isRead: inUnread is null);
    }

    public async Task<NotificationListResult> ListAsync(
        Guid tenantId,
        Guid userId,
        int page,
        int size,
        NotificationListFilter readStatus = NotificationListFilter.All,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        page = Math.Max(1, page);
        size = Math.Clamp(size, 1, 100);

        var indexKey = RedisNotificationKeys.Index(KeyPrefix, tenantId, userId);
        var unreadKey = RedisNotificationKeys.Unread(KeyPrefix, tenantId, userId);
        var readKey = RedisNotificationKeys.Read(KeyPrefix, tenantId, userId);

        RedisKey sourceKey = readStatus switch
        {
            NotificationListFilter.All => indexKey,
            NotificationListFilter.Unread => unreadKey,
            NotificationListFilter.Read => readKey,
            _ => throw new ArgumentOutOfRangeException(
                nameof(readStatus),
                readStatus,
                "Unsupported notification list filter.")
        };

        var totalItems = (int)await _db.SortedSetLengthAsync(sourceKey).ConfigureAwait(false);
        var totalPages = totalItems == 0 ? 0 : (int)Math.Ceiling(totalItems / (double)size);

        var start = (page - 1) * size;
        var stop = start + size - 1;
        var ids = await _db.SortedSetRangeByRankAsync(sourceKey, start, stop, Order.Descending)
            .ConfigureAwait(false);

        var parsed = ids
            .Select(v => Guid.TryParse(v.ToString(), out var id) ? id : (Guid?)null)
            .Where(id => id.HasValue)
            .Select(id => id!.Value)
            .ToArray();

        var keys = Array.ConvertAll(
            parsed, id => (RedisKey)RedisNotificationKeys.Item(KeyPrefix, tenantId, userId, id));

        var values = keys.Length == 0
            ? []
            : await _db.StringGetAsync(keys).ConfigureAwait(false);

        // Tab All: 1 RT batch — membership unread để suy isRead (ADR Q16).
        HashSet<string>? unreadOnPage = null;
        if (readStatus == NotificationListFilter.All && parsed.Length > 0)
        {
            unreadOnPage = await LoadUnreadMembershipAsync(unreadKey, parsed).ConfigureAwait(false);
        }

        var items = new List<SignalRNotificationItemDto>(parsed.Length);
        var orphans = new List<RedisValue>();

        for (var i = 0; i < parsed.Length; i++)
        {
            var member = parsed[i].ToString("D");
            var isRead = readStatus switch
            {
                NotificationListFilter.Unread => false,
                NotificationListFilter.Read => true,
                NotificationListFilter.All => unreadOnPage is null || !unreadOnPage.Contains(member),
                _ => false
            };

            var item = values[i].HasValue
                ? NotificationStoreJson.Deserialize(values[i]!, isRead)
                : null;

            if (item is null)
                orphans.Add(member);
            else
                items.Add(item);
        }

        if (orphans.Count > 0)
        {
            var members = orphans.ToArray();
            var batch = _db.CreateBatch();
            var removeIndexTask = batch.SortedSetRemoveAsync(indexKey, members);
            var removeUnreadTask = batch.SortedSetRemoveAsync(unreadKey, members);
            var removeReadTask = batch.SortedSetRemoveAsync(readKey, members);
            batch.Execute();
            await Task.WhenAll(removeIndexTask, removeUnreadTask, removeReadTask).ConfigureAwait(false);

            totalItems = (int)await _db.SortedSetLengthAsync(sourceKey).ConfigureAwait(false);
            totalPages = totalItems == 0 ? 0 : (int)Math.Ceiling(totalItems / (double)size);
        }

        var unread = await _db.SortedSetLengthAsync(unreadKey).ConfigureAwait(false);
        return new NotificationListResult
        {
            Data = items,
            UnreadCount = unread,
            Page = page,
            Size = size,
            TotalItems = totalItems,
            TotalPages = totalPages
        };
    }

    public async Task<long> GetUnreadCountAsync(
        Guid tenantId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var unreadKey = RedisNotificationKeys.Unread(KeyPrefix, tenantId, userId);
        return await _db.SortedSetLengthAsync(unreadKey).ConfigureAwait(false);
    }

    public Task MarkReadAsync(
        Guid tenantId,
        Guid userId,
        IReadOnlyCollection<Guid> notificationIds,
        CancellationToken cancellationToken = default) =>
        MoveReadStateAsync(
            tenantId,
            userId,
            notificationIds,
            toRead: true,
            cancellationToken);

    public Task MarkUnreadAsync(
        Guid tenantId,
        Guid userId,
        IReadOnlyCollection<Guid> notificationIds,
        CancellationToken cancellationToken = default) =>
        MoveReadStateAsync(
            tenantId,
            userId,
            notificationIds,
            toRead: false,
            cancellationToken);

    public async Task<long> MarkAllReadAsync(
        Guid tenantId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var unreadKey = RedisNotificationKeys.Unread(KeyPrefix, tenantId, userId);
        var readKey = RedisNotificationKeys.Read(KeyPrefix, tenantId, userId);

        // Chỉ snapshot member — không DEL cả key unread (tránh mất tin mới tới đồng thời).
        var entries = await _db.SortedSetRangeByRankWithScoresAsync(unreadKey).ConfigureAwait(false);
        if (entries.Length == 0)
            return 0;

        cancellationToken.ThrowIfCancellationRequested();

        var batch = _db.CreateBatch();
        var tasks = new List<Task>(entries.Length * 2);
        foreach (var entry in entries)
        {
            tasks.Add(batch.SortedSetAddAsync(readKey, entry.Element, entry.Score));
            tasks.Add(batch.SortedSetRemoveAsync(unreadKey, entry.Element));
        }

        batch.Execute();
        await Task.WhenAll(tasks).ConfigureAwait(false);
        return entries.Length;
    }

    /// <summary>
    /// Chuyển id giữa unread/read bằng pipeline. Score lấy từ ZSET nguồn (không rewrite item → không reset TTL).
    /// </summary>
    private async Task MoveReadStateAsync(
        Guid tenantId,
        Guid userId,
        IReadOnlyCollection<Guid> notificationIds,
        bool toRead,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(notificationIds);
        cancellationToken.ThrowIfCancellationRequested();

        var ids = notificationIds
            .Where(id => id != Guid.Empty)
            .Distinct()
            .ToArray();

        if (ids.Length == 0)
            return;

        var readKey = RedisNotificationKeys.Read(KeyPrefix, tenantId, userId);
        var unreadKey = RedisNotificationKeys.Unread(KeyPrefix, tenantId, userId);
        var indexKey = RedisNotificationKeys.Index(KeyPrefix, tenantId, userId);
        var sourceKey = toRead ? unreadKey : readKey;
        var destinationKey = toRead ? readKey : unreadKey;

        var members = Array.ConvertAll(ids, id => (RedisValue)id.ToString("D"));

        var scoreBatch = _db.CreateBatch();
        var sourceScoreTasks = new Task<double?>[members.Length];
        var indexScoreTasks = new Task<double?>[members.Length];
        for (var i = 0; i < members.Length; i++)
        {
            sourceScoreTasks[i] = scoreBatch.SortedSetScoreAsync(sourceKey, members[i]);
            indexScoreTasks[i] = scoreBatch.SortedSetScoreAsync(indexKey, members[i]);
        }

        scoreBatch.Execute();
        await Task.WhenAll(sourceScoreTasks.Concat(indexScoreTasks)).ConfigureAwait(false);

        var moves = new List<(RedisValue Member, double Score)>(members.Length);
        for (var i = 0; i < members.Length; i++)
        {
            var score = sourceScoreTasks[i].Result ?? indexScoreTasks[i].Result;
            if (score is null)
                continue;
            moves.Add((members[i], score.Value));
        }

        if (moves.Count == 0)
            return;

        cancellationToken.ThrowIfCancellationRequested();

        var batch = _db.CreateBatch();
        var tasks = new List<Task>(moves.Count * 2);
        foreach (var (member, score) in moves)
        {
            // ZADD đích trước ZREM nguồn (ADR D12).
            tasks.Add(batch.SortedSetAddAsync(destinationKey, member, score));
            tasks.Add(batch.SortedSetRemoveAsync(sourceKey, member));
        }

        batch.Execute();
        await Task.WhenAll(tasks).ConfigureAwait(false);
    }

    private async Task<HashSet<string>> LoadUnreadMembershipAsync(
        RedisKey unreadKey,
        Guid[] ids)
    {
        var batch = _db.CreateBatch();
        var scoreTasks = new Task<double?>[ids.Length];
        for (var i = 0; i < ids.Length; i++)
            scoreTasks[i] = batch.SortedSetScoreAsync(unreadKey, ids[i].ToString("D"));

        batch.Execute();
        await Task.WhenAll(scoreTasks).ConfigureAwait(false);

        var set = new HashSet<string>(StringComparer.Ordinal);
        for (var i = 0; i < ids.Length; i++)
        {
            if (scoreTasks[i].Result is not null)
                set.Add(ids[i].ToString("D"));
        }

        return set;
    }

    private static TimeSpan RemainingTtl(DateTimeOffset expiresAtUtc)
    {
        var ttl = expiresAtUtc - DateTimeOffset.UtcNow;
        return ttl < TimeSpan.FromSeconds(1) ? TimeSpan.FromSeconds(1) : ttl;
    }
}
