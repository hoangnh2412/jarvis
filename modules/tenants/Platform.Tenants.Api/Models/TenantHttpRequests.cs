using Platform.Tenants.Models;

namespace Platform.Tenants.Api.Models;

public sealed class CreateTenantHttpRequest
{
    public string? Code { get; set; }

    public string? Name { get; set; }

    public string? ConnectionString { get; set; }

    public Guid? ParentTenantId { get; set; }

    public CatalogAdminAccountInput? InitialAdmin { get; set; }
}

public sealed class UpdateTenantHttpRequest
{
    public string? Code { get; set; }

    public string? Name { get; set; }

    public string? ConnectionString { get; set; }
}

public sealed class SetTenantActiveHttpRequest
{
    public bool IsActive { get; set; }
}

public sealed class SetTenantParentHttpRequest
{
    public Guid? ParentTenantId { get; set; }
}
