using Elsa.Extensions;
using Elsa.Workflows;
using Elsa.Workflows.Models;

namespace Platform.Workflow.Idempotency;

/// <summary>
/// Base class cho các Elsa Workflow Activity cần đảm bảo tính Idempotent (chạy lại an toàn không sinh trùng lặp tác vụ DB/API).
/// </summary>
public abstract class IdempotentActivity : CodeActivity
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
            // Nếu không cung cấp IdempotencyKey, fallback kết hợp WorkflowInstanceId + ActivityNodeId
            key = $"{context.WorkflowExecutionContext.Id}:{context.NodeId}";
        }

        var ttl = TimeToLive.GetOrDefault(context);
        var executor = context.GetRequiredService<IWorkflowIdempotencyExecutor>();

        await executor.ExecuteAsync(key, async () =>
        {
            await ExecuteIdempotentAsync(context);
        }, ttl, context.CancellationToken);
    }

    /// <summary>
    /// Logic nghiệp vụ cập nhật DB / gọi API ngoại vi cần đảm bảo Idempotent.
    /// Sẽ chỉ thực thi 1 lần duy nhất cho cùng một IdempotencyKey.
    /// </summary>
    protected abstract ValueTask ExecuteIdempotentAsync(ActivityExecutionContext context);
}
