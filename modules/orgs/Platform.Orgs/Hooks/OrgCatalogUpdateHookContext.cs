using Platform.Orgs.Models;

namespace Platform.Orgs.Hooks;

public sealed class OrgCatalogUpdateHookContext<TOrg> : OrgCatalogHookContextBase
    where TOrg : class, IOrgCatalogEntity
{
    public UpdateOrgInput? Input { get; init; }

    public required TOrg Entity { get; init; }
}
