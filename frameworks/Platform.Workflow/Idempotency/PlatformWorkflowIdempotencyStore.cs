using System.Text.Json;
using Microsoft.Extensions.Logging;
using Platform.Caching.Redis;
using Platform.Workflow.Configuration;

namespace Platform.Workflow.Idempotency;

public sealed class PlatformWorkflowIdempotencyStore : IWorkflowIdempotencyStore
{
    private readonly IPlatformRedisStore _redisStore;
    private readonly WorkflowStoreOptions _options;
    private readonly ILogger<PlatformWorkflowIdempotencyStore> _logger;

    public PlatformWorkflowIdempotencyStore(IPlatformRedisStore redisStore, WorkflowStoreOptions options, ILogger<PlatformWorkflowIdempotencyStore> logger)
    {
        ArgumentNullException.ThrowIfNull(redisStore);
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(logger);
        _redisStore = redisStore;
        _options = options;
        _logger = logger;
    }

    public async Task<WorkflowIdempotencyRecord?> GetAsync(string key, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        var redisKey = BuildIdempotencyKey(key);
        var json = await _redisStore.GetStringAsync(redisKey, cancellationToken);
        if (json == null) return null;
        var record = JsonSerializer.Deserialize<WorkflowIdempotencyRecord>(json)
                     ?? throw new InvalidOperationException($"Idempotency record for key '{key}' is invalid.");
        if (record.ExpiresAtUtc.HasValue && record.ExpiresAtUtc.Value <= DateTimeOffset.UtcNow)
        {
            await _redisStore.DeleteAsync(redisKey, cancellationToken);
            return null;
        }
        return record;
    }

    public async Task<WorkflowIdempotencyLease?> TryAcquireAsync(string key, TimeSpan? ttl = null, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        var now = DateTimeOffset.UtcNow;
        var ownerToken = Guid.NewGuid().ToString("N");
        var record = new WorkflowIdempotencyRecord(
            key,
            IdempotencyStatus.Pending,
            CreatedAtUtc: now,
            ExpiresAtUtc: ttl.HasValue ? now.Add(ttl.Value) : null,
            OwnerToken: ownerToken);
        var storedValue = JsonSerializer.Serialize(record);
        var acquired = await _redisStore.SetStringIfNotExistsAsync(
            BuildIdempotencyKey(key),
            storedValue,
            ttl ?? _options.IdempotencyTtl,
            cancellationToken);
        if (acquired)
        {
            _logger.LogDebug("Acquired idempotency lock for key {Key}", key);
            return new WorkflowIdempotencyLease(key, ownerToken, storedValue);
        }

        _logger.LogWarning("Failed to acquire idempotency lock for key {Key} (already exists)", key);
        return null;
    }

    public async Task MarkCompletedAsync(WorkflowIdempotencyLease lease, string? resultJson = null, TimeSpan? ttl = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(lease);
        var now = DateTimeOffset.UtcNow;
        var record = new WorkflowIdempotencyRecord(
            lease.Key,
            IdempotencyStatus.Completed,
            ResultJson: resultJson,
            CreatedAtUtc: now,
            ExpiresAtUtc: ttl.HasValue ? now.Add(ttl.Value) : null,
            OwnerToken: lease.OwnerToken);
        var updated = await _redisStore.CompareAndSetStringAsync(
            BuildIdempotencyKey(lease.Key),
            lease.StoredValue,
            JsonSerializer.Serialize(record),
            ttl ?? _options.IdempotencyTtl,
            cancellationToken);
        if (!updated)
            throw new InvalidOperationException($"Idempotency lease for key '{lease.Key}' is no longer owned by this execution.");
    }

    public async Task MarkFailedAsync(WorkflowIdempotencyLease lease, string? errorMessage = null, bool allowRetry = true, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(lease);
        var redisKey = BuildIdempotencyKey(lease.Key);
        if (allowRetry)
        {
            var removed = await _redisStore.DeleteIfValueAsync(redisKey, lease.StoredValue, cancellationToken);
            if (!removed)
                throw new InvalidOperationException($"Idempotency lease for key '{lease.Key}' is no longer owned by this execution.");
            return;
        }

        var record = new WorkflowIdempotencyRecord(
            lease.Key,
            IdempotencyStatus.Failed,
            ErrorMessage: errorMessage,
            CreatedAtUtc: DateTimeOffset.UtcNow,
            OwnerToken: lease.OwnerToken);
        var updated = await _redisStore.CompareAndSetStringAsync(
            redisKey,
            lease.StoredValue,
            JsonSerializer.Serialize(record),
            _options.IdempotencyTtl,
            cancellationToken);
        if (!updated)
            throw new InvalidOperationException($"Idempotency lease for key '{lease.Key}' is no longer owned by this execution.");
    }

    private string BuildIdempotencyKey(string key) => $"{_options.KeyPrefix}:idempotency:{key}";
}
