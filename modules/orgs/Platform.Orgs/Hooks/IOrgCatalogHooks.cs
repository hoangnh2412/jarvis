namespace Platform.Orgs.Hooks;

/// <summary>
/// Extension points (hooks) cho lifecycle ghi catalog Org.
/// Thứ tự gọi: <b>Validate</b> → áp invariant platform → <b>Prepare</b> → lưu → <b>Success</b> (nếu thành công);
/// khi lỗi: <b>Error</b> (sau khi gán <see cref="OrgCatalogHookContextBase.Exception"/>) → rethrow → <b>Finally</b> (luôn).
/// </summary>
public interface IOrgCatalogHooks<TOrg>
    where TOrg : class, IOrgManagementEntity
{
    // --- Validate (ném exception để hủy thao tác) ---

    Task ValidateCreateAsync(OrgCatalogCreateHookContext<TOrg> context, CancellationToken cancellationToken = default);

    Task ValidateUpdateAsync(OrgCatalogUpdateHookContext<TOrg> context, CancellationToken cancellationToken = default);

    Task ValidateDeleteAsync(OrgCatalogDeleteHookContext<TOrg> context, CancellationToken cancellationToken = default);

    // --- Prepare (trước SaveChanges; map input / cột host sau khi platform gán cột chuẩn) ---

    Task PrepareCreateAsync(OrgCatalogCreateHookContext<TOrg> context, CancellationToken cancellationToken = default);

    Task PrepareUpdateAsync(OrgCatalogUpdateHookContext<TOrg> context, CancellationToken cancellationToken = default);

    Task PrepareDeleteAsync(OrgCatalogDeleteHookContext<TOrg> context, CancellationToken cancellationToken = default);

    // --- Success (sau SaveChanges thành công) ---

    Task SuccessCreateAsync(OrgCatalogCreateHookContext<TOrg> context, CancellationToken cancellationToken = default);

    Task SuccessUpdateAsync(OrgCatalogUpdateHookContext<TOrg> context, CancellationToken cancellationToken = default);

    Task SuccessDeleteAsync(OrgCatalogDeleteHookContext<TOrg> context, CancellationToken cancellationToken = default);

    // --- Error (khi pipeline lỗi; <see cref="OrgCatalogHookContextBase.Exception"/> đã được gán) ---

    Task ErrorCreateAsync(OrgCatalogCreateHookContext<TOrg> context, CancellationToken cancellationToken = default);

    Task ErrorUpdateAsync(OrgCatalogUpdateHookContext<TOrg> context, CancellationToken cancellationToken = default);

    Task ErrorDeleteAsync(OrgCatalogDeleteHookContext<TOrg> context, CancellationToken cancellationToken = default);

    // --- Finally (luôn chạy; kiểm tra <see cref="OrgCatalogHookContextBase.Succeeded"/> / <see cref="OrgCatalogHookContextBase.Exception"/>) ---

    Task FinallyCreateAsync(OrgCatalogCreateHookContext<TOrg> context, CancellationToken cancellationToken = default);

    Task FinallyUpdateAsync(OrgCatalogUpdateHookContext<TOrg> context, CancellationToken cancellationToken = default);

    Task FinallyDeleteAsync(OrgCatalogDeleteHookContext<TOrg> context, CancellationToken cancellationToken = default);
}
