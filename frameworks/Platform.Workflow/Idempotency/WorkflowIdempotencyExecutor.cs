using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace Platform.Workflow.Idempotency;

/// <summary>
/// Triển khai mặc định của <see cref="IWorkflowIdempotencyExecutor"/>.
/// </summary>
public sealed class WorkflowIdempotencyExecutor(
    IWorkflowIdempotencyStore store,
    ILogger<WorkflowIdempotencyExecutor> logger) : IWorkflowIdempotencyExecutor
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<TResult?> ExecuteAsync<TResult>(
        string idempotencyKey,
        Func<Task<TResult>> executeAction,
        TimeSpan? ttl = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(idempotencyKey);
        ArgumentNullException.ThrowIfNull(executeAction);

        var existing = await store.GetAsync(idempotencyKey, cancellationToken);
        if (existing is { Status: IdempotencyStatus.Completed })
        {
            logger.LogInformation("Idempotency key '{Key}' has already been executed successfully. Returning cached result.", idempotencyKey);
            if (string.IsNullOrWhiteSpace(existing.ResultJson))
            {
                return default;
            }

            return JsonSerializer.Deserialize<TResult>(existing.ResultJson, JsonOptions);
        }

        var lease = await store.TryAcquireAsync(idempotencyKey, ttl, cancellationToken);
        if (lease is null)
        {
            // Kiểm tra lại nếu key vừa hoàn thành trong lúc chờ
            var record = await store.GetAsync(idempotencyKey, cancellationToken);
            if (record is { Status: IdempotencyStatus.Completed })
            {
                logger.LogInformation("Idempotency key '{Key}' completed by another concurrent execution.", idempotencyKey);
                return string.IsNullOrWhiteSpace(record.ResultJson)
                    ? default
                    : JsonSerializer.Deserialize<TResult>(record.ResultJson, JsonOptions);
            }

            throw new InvalidOperationException($"Idempotency key '{idempotencyKey}' is currently being processed by another execution.");
        }

        var actionCompleted = false;
        try
        {
            var result = await executeAction();
            actionCompleted = true;
            var resultJson = result is not null ? JsonSerializer.Serialize(result, JsonOptions) : null;
            await store.MarkCompletedAsync(lease, resultJson, ttl, cancellationToken);
            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Execution failed for idempotency key '{Key}'. Marking as failed.", idempotencyKey);
            if (!actionCompleted)
                await store.MarkFailedAsync(lease, ex.Message, allowRetry: true, cancellationToken);
            throw;
        }
    }

    public async Task ExecuteAsync(
        string idempotencyKey,
        Func<Task> executeAction,
        TimeSpan? ttl = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(idempotencyKey);
        ArgumentNullException.ThrowIfNull(executeAction);

        var existing = await store.GetAsync(idempotencyKey, cancellationToken);
        if (existing is { Status: IdempotencyStatus.Completed })
        {
            logger.LogInformation("Idempotency key '{Key}' has already been executed successfully. Skipping execution.", idempotencyKey);
            return;
        }

        var lease = await store.TryAcquireAsync(idempotencyKey, ttl, cancellationToken);
        if (lease is null)
        {
            var record = await store.GetAsync(idempotencyKey, cancellationToken);
            if (record is { Status: IdempotencyStatus.Completed })
            {
                logger.LogInformation("Idempotency key '{Key}' completed by another concurrent execution.", idempotencyKey);
                return;
            }

            throw new InvalidOperationException($"Idempotency key '{idempotencyKey}' is currently being processed by another execution.");
        }

        var actionCompleted = false;
        try
        {
            await executeAction();
            actionCompleted = true;
            await store.MarkCompletedAsync(lease, resultJson: null, ttl, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Execution failed for idempotency key '{Key}'. Marking as failed.", idempotencyKey);
            if (!actionCompleted)
                await store.MarkFailedAsync(lease, ex.Message, allowRetry: true, cancellationToken);
            throw;
        }
    }
}
