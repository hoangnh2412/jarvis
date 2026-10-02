using System.Collections.Concurrent;

namespace Platform.Workflow.Idempotency;

/// <summary>
/// Triển khai in-memory mặc định của <see cref="IWorkflowIdempotencyStore"/>.
/// Có thể thay thế bằng Redis hoặc Database store trong môi trường distributed.
/// </summary>
public sealed class MemoryWorkflowIdempotencyStore : IWorkflowIdempotencyStore
{
    private readonly ConcurrentDictionary<string, WorkflowIdempotencyRecord> _records = new();

    public Task<WorkflowIdempotencyRecord?> GetAsync(string key, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        if (_records.TryGetValue(key, out var record))
        {
            if (record.ExpiresAtUtc.HasValue && record.ExpiresAtUtc.Value <= DateTimeOffset.UtcNow)
            {
                _records.TryRemove(key, out _);
                return Task.FromResult<WorkflowIdempotencyRecord?>(null);
            }

            return Task.FromResult<WorkflowIdempotencyRecord?>(record);
        }

        return Task.FromResult<WorkflowIdempotencyRecord?>(null);
    }

    public Task<WorkflowIdempotencyLease?> TryAcquireAsync(string key, TimeSpan? ttl = null, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        var now = DateTimeOffset.UtcNow;
        var expiresAt = ttl.HasValue ? now.Add(ttl.Value) : (DateTimeOffset?)null;

        var newRecord = new WorkflowIdempotencyRecord(
            key,
            IdempotencyStatus.Pending,
            CreatedAtUtc: now,
            ExpiresAtUtc: expiresAt,
            OwnerToken: Guid.NewGuid().ToString("N"));
        var lease = new WorkflowIdempotencyLease(key, newRecord.OwnerToken!, string.Empty);

        while (true)
        {
            if (_records.TryGetValue(key, out var existing))
            {
                if (existing.ExpiresAtUtc.HasValue && existing.ExpiresAtUtc.Value <= now)
                {
                    if (_records.TryUpdate(key, newRecord, existing))
                        return Task.FromResult<WorkflowIdempotencyLease?>(lease);

                    continue;
                }

                return Task.FromResult<WorkflowIdempotencyLease?>(null);
            }

            if (_records.TryAdd(key, newRecord))
            {
                return Task.FromResult<WorkflowIdempotencyLease?>(lease);
            }
        }
    }

    public Task MarkCompletedAsync(WorkflowIdempotencyLease lease, string? resultJson = null, TimeSpan? ttl = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(lease);

        var now = DateTimeOffset.UtcNow;
        var expiresAt = ttl.HasValue ? now.Add(ttl.Value) : (DateTimeOffset?)null;

        var completedRecord = new WorkflowIdempotencyRecord(
            lease.Key,
            IdempotencyStatus.Completed,
            ResultJson: resultJson,
            CreatedAtUtc: now,
            ExpiresAtUtc: expiresAt,
            OwnerToken: lease.OwnerToken);

        while (_records.TryGetValue(lease.Key, out var existing))
        {
            if (!string.Equals(existing.OwnerToken, lease.OwnerToken, StringComparison.Ordinal))
                throw new InvalidOperationException($"Idempotency lease for key '{lease.Key}' is no longer owned by this execution.");

            if (_records.TryUpdate(lease.Key, completedRecord, existing))
                return Task.CompletedTask;
        }

        throw new InvalidOperationException($"Idempotency lease for key '{lease.Key}' has expired or was removed.");
    }

    public Task MarkFailedAsync(WorkflowIdempotencyLease lease, string? errorMessage = null, bool allowRetry = true, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(lease);

        if (!_records.TryGetValue(lease.Key, out var existing) ||
            !string.Equals(existing.OwnerToken, lease.OwnerToken, StringComparison.Ordinal))
            throw new InvalidOperationException($"Idempotency lease for key '{lease.Key}' is no longer owned by this execution.");

        if (allowRetry)
        {
            _records.TryRemove(new KeyValuePair<string, WorkflowIdempotencyRecord>(lease.Key, existing));
        }
        else
        {
            var now = DateTimeOffset.UtcNow;
            var failedRecord = new WorkflowIdempotencyRecord(
                lease.Key,
                IdempotencyStatus.Failed,
                ErrorMessage: errorMessage,
                CreatedAtUtc: now,
                OwnerToken: lease.OwnerToken);

            if (!_records.TryUpdate(lease.Key, failedRecord, existing))
                throw new InvalidOperationException($"Idempotency lease for key '{lease.Key}' changed during failure handling.");
        }

        return Task.CompletedTask;
    }
}
