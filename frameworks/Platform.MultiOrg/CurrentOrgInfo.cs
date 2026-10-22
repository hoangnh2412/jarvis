using Platform.DDD.Domain.Services;

namespace Platform.MultiOrg;

/// <summary><see cref="ICurrentOrgIdentity"/> mặc định.</summary>
public class CurrentOrgInfo : ICurrentOrgIdentity
{
    public Guid? OrgId { get; init; }

    public string? Code { get; init; }

    public string? Name { get; init; }
}
