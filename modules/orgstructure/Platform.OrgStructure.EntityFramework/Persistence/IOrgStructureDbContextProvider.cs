using Microsoft.EntityFrameworkCore;

namespace Platform.OrgStructure.EntityFramework.Persistence;

/// <summary>
/// Cửa truy cập DbContext chứa các bảng org-structure của host (unit, pháp nhân, danh mục, thành viên).
/// Host đăng ký adapter trên unit of work của mình; service platform dùng interface này thay vì DbContext cụ thể.
/// </summary>
public interface IOrgStructureDbContextProvider
{
    DbContext DbContext { get; }
}
