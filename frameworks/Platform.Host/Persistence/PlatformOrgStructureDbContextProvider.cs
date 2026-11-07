using Microsoft.EntityFrameworkCore;
using Platform.OrgStructure.EntityFramework.Persistence;

namespace Platform.Host.Persistence;

/// <summary>Adapter mặc định: bảng org-structure nằm trên DbContext tenant do host đăng ký.</summary>
public sealed class PlatformOrgStructureDbContextProvider(DbContext db) : IOrgStructureDbContextProvider
{
    public DbContext DbContext => db;
}
