namespace Platform.Workflow.Abstractions;

/// <summary>
/// Service để quản lý workflow definitions.
/// </summary>
public interface IWorkflowDefinitionService
{
    /// <summary>
    /// Lấy danh sách tất cả workflow definition IDs.
    /// </summary>
    Task<IReadOnlyCollection<string>> ListAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Lấy thông tin workflow definition theo ID.
    /// </summary>
    /// <param name="definitionId">ID của workflow definition</param>
    /// <returns>Workflow definition metadata, hoặc null nếu không tìm thấy</returns>
    Task<WorkflowDefinitionMetadata?> GetAsync(string definitionId, CancellationToken cancellationToken = default);
    /// <summary>
    /// Lấy JSON definition Elsa (graph/root) theo <paramref name="definitionId"/>.
    /// Dùng cho FE render sơ đồ; không tạo instance.
    /// </summary>
    /// <param name="definitionId">Elsa definition id (ổn định qua các version).</param>
    /// <param name="version">Version cụ thể; null = published (hoặc latest nếu chưa publish).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<WorkflowDefinitionDocument?> GetDocumentAsync(
        string definitionId,
        int? version = null,
        CancellationToken cancellationToken = default);
    /// <summary>
    /// Xóa workflow definition.
    /// </summary>
    /// <param name="definitionId">ID của workflow definition cần xóa</param>
    /// <returns>True nếu xóa thành công, false nếu không tìm thấy</returns>
    Task<bool> DeleteAsync(string definitionId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Metadata của một workflow definition.
/// </summary>
public sealed record WorkflowDefinitionMetadata(
    string DefinitionId,
    string? Name = null,
    string? Description = null,
    int? Version = null,
    bool IsPublished = false);

/// <summary>
/// Document Elsa workflow definition — metadata + JSON graph để FE hiển thị.
/// <see cref="Definition"/> là object serializable (thường là activity root / WorkflowDefinitionModel).
/// </summary>
public sealed record WorkflowDefinitionDocument(
    string DefinitionId,
    string? Name,
    string? Description,
    int Version,
    bool IsPublished,
    bool IsLatest,
    object? Definition);

