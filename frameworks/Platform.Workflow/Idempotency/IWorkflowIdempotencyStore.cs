namespace Platform.Workflow.Idempotency;

/// <summary>
/// Store lưu trữ và đối soát Idempotency Key cho các bước thực thi trong Workflow hoặc hệ thống bên ngoài.
/// </summary>
public interface IWorkflowIdempotencyStore
{
    /// <summary>
    /// Thử lấy bản ghi Idempotency đã tồn tại theo key.
    /// </summary>
    Task<WorkflowIdempotencyRecord?> GetAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Thử đăng ký key ở trạng thái Pending. Trả về lease nếu đăng ký mới thành công.
    /// </summary>
    Task<WorkflowIdempotencyLease?> TryAcquireAsync(string key, TimeSpan? ttl = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Đánh dấu key hoàn thành thành công và lưu kèm kết quả (JSON).
    /// </summary>
    Task MarkCompletedAsync(WorkflowIdempotencyLease lease, string? resultJson = null, TimeSpan? ttl = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Đánh dấu key thất bại để cho phép thử lại hoặc ghi nhận lỗi.
    /// </summary>
    Task MarkFailedAsync(WorkflowIdempotencyLease lease, string? errorMessage = null, bool allowRetry = true, CancellationToken cancellationToken = default);
}
