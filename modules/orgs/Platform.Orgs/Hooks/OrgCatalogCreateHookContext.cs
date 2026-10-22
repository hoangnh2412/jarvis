using Platform.Orgs.Models;

namespace Platform.Orgs.Hooks;

public sealed class OrgCatalogCreateHookContext<TOrg> : OrgCatalogHookContextBase
    where TOrg : class, IOrgCatalogEntity
{
    public required CreateOrgInput Input { get; init; }

    public required TOrg Entity { get; init; }
}
