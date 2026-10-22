namespace Platform.DDD.Domain.Services;

/// <summary>Profile organization tối thiểu cho ambient working org.</summary>
public interface ICurrentOrgIdentity
{
    Guid? OrgId { get; }

    string? Code { get; }

    string? Name { get; }
}
