namespace Platform.Orgs.Api.Models;

public sealed class CreateOrgHttpRequest
{
    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;
}

public sealed class UpdateOrgHttpRequest
{
    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;
}

public sealed class SetOrgActiveHttpRequest
{
    public bool IsActive { get; set; }
}
