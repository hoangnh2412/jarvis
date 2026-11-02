using Platform.DDD.Domain.Entities;

namespace Platform.OrgStructure;

/// <summary>Dòng danh mục đặt tên theo Org (loại đơn vị, cấp chuyên môn, vị trí công việc).</summary>
public interface IOrgNamedCatalogEntity : IEntity<Guid>
{
    Guid OrgId { get; set; }

    string Code { get; set; }

    string Name { get; set; }

    int SortOrder { get; set; }
}

/// <summary>Loại đơn vị: danh mục theo Org, mã lưu trên <see cref="IOrganizationalUnitEntity.UnitType"/>.</summary>
public interface IOrgUnitTypeEntity : IOrgNamedCatalogEntity
{
    string Icon { get; set; }
}

/// <summary>Thành viên: user trên đơn vị + vị trí công việc + cấp chuyên môn + chức vụ (tùy chọn).</summary>
public interface IOrgUnitMemberEntity : IEntity<Guid>
{
    Guid OrgId { get; set; }

    Guid OrganizationalUnitId { get; set; }

    Guid UserId { get; set; }

    Guid JobPositionId { get; set; }

    Guid JobGradeId { get; set; }

    Guid? JobTitleId { get; set; }

    bool IsActive { get; set; }
}

/// <summary>Pháp nhân kèm hồ sơ mở rộng dạng JSON (cùng shape form thông tin doanh nghiệp).</summary>
public interface ILegalEntityProfileEntity : ILegalEntityEntity
{
    string? DetailsJson { get; set; }
}
