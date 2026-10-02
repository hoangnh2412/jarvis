namespace Platform.Workflow.Idempotency;

/// <summary>
/// Executor điều phối thực thi các tác vụ / node workflow theo cơ chế Idempotent (chống trùng lặp).
/// </summary>
public interface IWorkflowIdempotencyExecutor
{
    /// <summary>
    /// Thực thi một tác vụ có trả về kết quả theo Idempotency Key.
    /// Nếu key đã hoàn thành trước đó, trả về kết quả đã lưu mà không chạy lại action.
    /// </summary>
    Task<TResult?> ExecuteAsync<TResult>(
        string idempotencyKey,
        Func<Task<TResult>> executeAction,
        TimeSpan? ttl = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Thực thi một tác vụ không trả về kết quả theo Idempotency Key.
    /// Nếu key đã hoàn thành trước đó, bỏ qua mà không chạy lại action.
    /// </summary>
    Task ExecuteAsync(
        string idempotencyKey,
        Func<Task> executeAction,
        TimeSpan? ttl = null,
        CancellationToken cancellationToken = default);
}
