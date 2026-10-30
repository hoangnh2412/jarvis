namespace Platform.Orgs.Hooks;

public sealed class OrgCatalogDeleteHookContext<TOrg> : OrgCatalogHookContextBase
    where TOrg : class, IOrgManagementEntity
{
    public required TOrg Entity { get; init; }
}
