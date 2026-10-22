namespace Platform.Orgs.Abstractions;

/// <summary>
/// Đọc và kiểm tra catalog Org cho module khác (IdentityAdmin, OrgStructure).
/// Caller truyền <c>tenantId</c> tường minh — không lấy ambient.
/// </summary>
public interface IOrgCatalog
{
    Task<OrgDescriptor?> GetAsync(Guid orgId, Guid tenantId, CancellationToken cancellationToken = default);

    Task<OrgDescriptor?> GetByCodeAsync(string code, Guid tenantId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<OrgDescriptor>> ListAsync(
        Guid tenantId,
        bool? isActive = null,
        string? search = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Org phải tồn tại trong tenant và đang active. Không thấy thì NotFound; inactive thì Conflict.
    /// </summary>
    Task ValidateForAssignmentAsync(Guid orgId, Guid tenantId, CancellationToken cancellationToken = default);
}
