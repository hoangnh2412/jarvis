namespace Platform.Workflow.Idempotency;

public enum IdempotencyStatus
{
    Pending = 0,
    Completed = 1,
    Failed = 2
}

public sealed record WorkflowIdempotencyRecord(
    string Key,
    IdempotencyStatus Status,
    string? ResultJson = null,
    string? ErrorMessage = null,
    DateTimeOffset CreatedAtUtc = default,
    DateTimeOffset? ExpiresAtUtc = null,
    string? OwnerToken = null);

public sealed record WorkflowIdempotencyLease(
    string Key,
    string OwnerToken,
    string StoredValue);
