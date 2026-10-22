using Platform.DDD.Domain.Services;

namespace Platform.Orgs.Services;

/// <summary>Profile org map từ <see cref="Abstractions.OrgDescriptor"/> — dùng với <see cref="OrgCatalogCurrentOrgStore{TTenant}"/>.</summary>
public sealed class OrgCatalogOrgProfile : ICurrentOrgIdentity
{
    public Guid? OrgId { get; init; }

    public string? Code { get; init; }

    public string? Name { get; init; }
}
