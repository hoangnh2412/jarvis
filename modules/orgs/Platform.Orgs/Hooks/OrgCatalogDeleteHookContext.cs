namespace Platform.Orgs.Hooks;

public sealed class OrgCatalogDeleteHookContext<TOrg> : OrgCatalogHookContextBase
    where TOrg : class, IOrgCatalogEntity
{
    public required TOrg Entity { get; init; }
}
