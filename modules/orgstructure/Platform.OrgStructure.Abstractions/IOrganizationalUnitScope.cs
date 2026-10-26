namespace Platform.OrgStructure.Abstractions;

/// <summary>
/// Phạm vi đơn vị trên bản ghi nghiệp vụ và trên user.
/// Tên chuẩn là <c>OrganizationalUnitId</c> và <c>UnitType</c>.
/// </summary>
public interface IOrganizationalUnitScope
{
    Guid? OrganizationalUnitId { get; }

    string? UnitType { get; }
}
