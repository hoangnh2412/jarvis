namespace Platform.Orgs.Hooks;

/// <summary>Hooks mặc định — no-op.</summary>
public class DefaultOrgCatalogHooks<TOrg> : IOrgCatalogHooks<TOrg>
    where TOrg : class, IOrgCatalogEntity
{
    public virtual Task ValidateCreateAsync(
        OrgCatalogCreateHookContext<TOrg> context,
        CancellationToken cancellationToken = default) => Task.CompletedTask;

    public virtual Task ValidateUpdateAsync(
        OrgCatalogUpdateHookContext<TOrg> context,
        CancellationToken cancellationToken = default) => Task.CompletedTask;

    public virtual Task ValidateDeleteAsync(
        OrgCatalogDeleteHookContext<TOrg> context,
        CancellationToken cancellationToken = default) => Task.CompletedTask;

    public virtual Task PrepareCreateAsync(
        OrgCatalogCreateHookContext<TOrg> context,
        CancellationToken cancellationToken = default) => Task.CompletedTask;

    public virtual Task PrepareUpdateAsync(
        OrgCatalogUpdateHookContext<TOrg> context,
        CancellationToken cancellationToken = default) => Task.CompletedTask;

    public virtual Task PrepareDeleteAsync(
        OrgCatalogDeleteHookContext<TOrg> context,
        CancellationToken cancellationToken = default) => Task.CompletedTask;

    public virtual Task SuccessCreateAsync(
        OrgCatalogCreateHookContext<TOrg> context,
        CancellationToken cancellationToken = default) => Task.CompletedTask;

    public virtual Task SuccessUpdateAsync(
        OrgCatalogUpdateHookContext<TOrg> context,
        CancellationToken cancellationToken = default) => Task.CompletedTask;

    public virtual Task SuccessDeleteAsync(
        OrgCatalogDeleteHookContext<TOrg> context,
        CancellationToken cancellationToken = default) => Task.CompletedTask;

    public virtual Task ErrorCreateAsync(
        OrgCatalogCreateHookContext<TOrg> context,
        CancellationToken cancellationToken = default) => Task.CompletedTask;

    public virtual Task ErrorUpdateAsync(
        OrgCatalogUpdateHookContext<TOrg> context,
        CancellationToken cancellationToken = default) => Task.CompletedTask;

    public virtual Task ErrorDeleteAsync(
        OrgCatalogDeleteHookContext<TOrg> context,
        CancellationToken cancellationToken = default) => Task.CompletedTask;

    public virtual Task FinallyCreateAsync(
        OrgCatalogCreateHookContext<TOrg> context,
        CancellationToken cancellationToken = default) => Task.CompletedTask;

    public virtual Task FinallyUpdateAsync(
        OrgCatalogUpdateHookContext<TOrg> context,
        CancellationToken cancellationToken = default) => Task.CompletedTask;

    public virtual Task FinallyDeleteAsync(
        OrgCatalogDeleteHookContext<TOrg> context,
        CancellationToken cancellationToken = default) => Task.CompletedTask;
}
