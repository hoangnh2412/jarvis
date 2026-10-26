namespace Platform.Tenants.Models;

public sealed class CreateTenantInput
{
    public string? Code { get; set; }

    public string? Name { get; set; }

    public string? ConnectionString { get; set; }

    public Guid? ParentTenantId { get; set; }

    public CatalogAdminAccountInput? InitialAdmin { get; set; }
}

public sealed class UpdateTenantInput
{
    public string? Code { get; set; }

    public string? Name { get; set; }

    public string? ConnectionString { get; set; }
}

public sealed class ListTenantsQuery
{
    public bool? IsActive { get; set; }

    public string? Search { get; set; }
}
