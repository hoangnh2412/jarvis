using Platform.Orgs.Hooks;
using Platform.Orgs.Models;

namespace Sample.Organization;

public class SampleCreateOrgInput : CreateOrgInput
{
    public string? TaxCode { get; set; }
}

public class SampleUpdateOrgInput : UpdateOrgInput
{
    public string? TaxCode { get; set; }
}

/// <summary>Gán <c>TaxCode</c> trong hook <see cref="IOrgCatalogHooks{TOrg}.PrepareCreateAsync"/> / PrepareUpdate.</summary>
public sealed class SampleOrgCatalogHooks : DefaultOrgCatalogHooks<Sample.Entities.Org>
{
    public override Task PrepareCreateAsync(
        OrgCatalogCreateHookContext<Sample.Entities.Org> context,
        CancellationToken cancellationToken = default)
    {
        if (context.Input is SampleCreateOrgInput sample)
            context.Entity.TaxCode = Normalize(sample.TaxCode);

        return Task.CompletedTask;
    }

    public override Task PrepareUpdateAsync(
        OrgCatalogUpdateHookContext<Sample.Entities.Org> context,
        CancellationToken cancellationToken = default)
    {
        if (context.Input is SampleUpdateOrgInput sample)
            context.Entity.TaxCode = Normalize(sample.TaxCode);

        return Task.CompletedTask;
    }

    private static string? Normalize(string? taxCode)
    {
        var trimmed = taxCode?.Trim();
        return string.IsNullOrEmpty(trimmed) ? null : trimmed;
    }
}
