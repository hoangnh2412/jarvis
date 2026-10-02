namespace Platform.Workflow.Multitenancy;

/// <summary>
/// Trừu tượng hóa việc lấy Tenant ID cho Elsa Workflow mà không phụ thuộc trực tiếp compile-time vào Platform.DDD.Domain.
/// </summary>
public interface IWorkflowTenantAccessor
{
    /// <summary>
    /// Lấy Tenant ID hiện tại (dạng chuỗi), hoặc null nếu đang ở ngữ cảnh host/mặc định.
    /// </summary>
    string? GetTenantId();
}
