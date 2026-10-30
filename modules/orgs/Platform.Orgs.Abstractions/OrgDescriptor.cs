namespace Platform.Orgs.Abstractions;

/// <summary>
/// Snapshot cột chuẩn cho SPI (<see cref="IOrgCatalog"/>). Cột host: read model riêng + <c>IOrgCatalogReadMapper</c>.
/// </summary>
public sealed class OrgDescriptor
{
    public Guid Id { get; init; }

    public Guid TenantId { get; init; }

    public string Code { get; init; } = string.Empty;

    public string Name { get; init; } = string.Empty;

    public bool IsActive { get; init; }

    public Guid? ParentId { get; init; }
}
