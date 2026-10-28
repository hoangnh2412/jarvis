namespace Platform.OrgStructure.Services;

/// <summary>Phạm vi đơn vị của request khi bật <c>OrgStructure:FilterABACByOrgStructure</c>.</summary>
public interface IRequestOrgUnitScope
{
    bool FilterEnabled { get; }

    /// <summary>
    /// Cờ tắt thì <c>null</c>. Cờ bật thì bắt <c>X-Org-Unit-Id</c> và <c>X-Org-Unit-Type</c>,
    /// đơn vị phải thuộc <paramref name="orgId"/> và loại trùng header.
    /// </summary>
    Task<RequestOrgUnitScope?> ResolveAsync(Guid? orgId, CancellationToken cancellationToken = default);
}

public sealed class RequestOrgUnitScope
{
    public Guid OrganizationalUnitId { get; init; }

    public string UnitType { get; init; } = string.Empty;

    public string Path { get; init; } = string.Empty;
}
