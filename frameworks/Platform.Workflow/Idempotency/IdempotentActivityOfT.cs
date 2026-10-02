using Elsa.Extensions;
using Elsa.Workflows;
using Elsa.Workflows.Models;

namespace Platform.Workflow.Idempotency;

/// <summary>
/// Base class cho các Elsa Workflow Activity có trả về kết quả và cần đảm bảo tính Idempotent.
/// </summary>
public abstract class IdempotentActivity<TResult> : CodeActivity<TResult>
{
    /// <summary>
    /// Key định danh duy nhất của tác vụ (ví dụ: OrderId_Step1, TransactionId, CorrelationId...).
    /// </summary>
    public Input<string> IdempotencyKey { get; set; } = default!;

    /// <summary>
    /// Thời gian sống của Idempotency Key (mặc định 24h).
    /// </summary>
    public Input<TimeSpan?> TimeToLive { get; set; } = new(TimeSpan.FromHours(24));

    protected override async ValueTask ExecuteAsync(ActivityExecutionContext context)
    {
        var key = IdempotencyKey.GetOrDefault(context);
        if (string.IsNullOrWhiteSpace(key))
        {
            key = $"{context.WorkflowExecutionContext.Id}:{context.NodeId}";
        }

        var ttl = TimeToLive.GetOrDefault(context);
        var executor = context.GetRequiredService<IWorkflowIdempotencyExecutor>();

        var result = await executor.ExecuteAsync(key, async () =>
        {
            return await ExecuteIdempotentAsync(context);
        }, ttl, context.CancellationToken);

        context.SetResult(result);
    }

    /// <summary>
    /// Logic nghiệp vụ cập nhật DB / gọi API ngoại vi cần đảm bảo Idempotent.
    /// </summary>
    protected abstract ValueTask<TResult> ExecuteIdempotentAsync(ActivityExecutionContext context);
}
