namespace Platform.Workflow.Abstractions;

/// <summary>
/// Service để quản lý workflow instances.
/// </summary>
public interface IWorkflowInstanceService
{
    /// <summary>
    /// Lấy danh sách tất cả workflow instance IDs.
    /// </summary>
    Task<IReadOnlyCollection<string>> ListAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Lấy thông tin workflow instance theo ID.
    /// </summary>
    /// <param name="instanceId">ID của workflow instance</param>
    /// <returns>Workflow instance metadata, hoặc null nếu không tìm thấy</returns>
    Task<WorkflowInstanceMetadata?> GetAsync(string instanceId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Xóa workflow instance.
    /// </summary>
    /// <param name="instanceId">ID của workflow instance cần xóa</param>
    /// <returns>True nếu xóa thành công, false nếu không tìm thấy</returns>
    Task<bool> DeleteAsync(string instanceId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Metadata của một workflow instance.
/// </summary>
public sealed record WorkflowInstanceMetadata(
    string InstanceId,
    string? DefinitionId = null,
    string? Status = null,
    DateTimeOffset? CreatedAt = null,
    DateTimeOffset? CompletedAt = null);
