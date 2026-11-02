using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Platform.OrgStructure.EntityFramework.Persistence;
using Platform.OrgStructure.Services;

namespace Platform.OrgStructure.EntityFramework.Services;

/// <summary>CRUD loại đơn vị theo Org; seed mặc định từ <see cref="OrgStructureCatalogOptions"/> khi Org chưa có dòng.</summary>
public sealed class OrgUnitTypeService<TUnitType, TUnit>(
    IOrgStructureDbContextProvider dbProvider,
    IOptions<OrgStructureCatalogOptions> options) : IOrgUnitTypeService
    where TUnitType : class, IOrgUnitTypeEntity
    where TUnit : class, IOrganizationalUnitEntity
{
    public async Task<IReadOnlyList<OrgUnitTypeDescriptor>> ListAsync(Guid orgId, CancellationToken cancellationToken)
    {
        var db = dbProvider.DbContext;
        var rows = await db.Set<TUnitType>()
            .AsNoTracking()
            .Where(x => x.OrgId == orgId)
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.Name)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        if (rows.Count > 0)
            return rows.Select(Map).ToList();

        await SeedDefaultsAsync(db, orgId, cancellationToken).ConfigureAwait(false);
        rows = await db.Set<TUnitType>()
            .AsNoTracking()
            .Where(x => x.OrgId == orgId)
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.Name)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        return rows.Select(Map).ToList();
    }

    public async Task<OrgUnitTypeDescriptor> CreateAsync(
        Guid orgId,
        string code,
        string name,
        string icon,
        CancellationToken cancellationToken)
    {
        var db = dbProvider.DbContext;
        var normalizedCode = NormalizeCode(code);
        var exists = await db.Set<TUnitType>().AnyAsync(
            x => x.OrgId == orgId && x.Code == normalizedCode,
            cancellationToken).ConfigureAwait(false);
        if (exists)
            throw new InvalidOperationException("Mã loại đơn vị đã tồn tại trong tổ chức.");

        var maxSort = await db.Set<TUnitType>()
            .Where(x => x.OrgId == orgId)
            .Select(x => (int?)x.SortOrder)
            .MaxAsync(cancellationToken)
            .ConfigureAwait(false) ?? -1;

        var entity = Activator.CreateInstance<TUnitType>();
        entity.Id = Guid.NewGuid();
        entity.OrgId = orgId;
        entity.Code = normalizedCode;
        entity.Name = name.Trim();
        entity.Icon = options.Value.NormalizeIcon(icon);
        entity.SortOrder = maxSort + 1;
        db.Set<TUnitType>().Add(entity);
        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Map(entity);
    }

    public async Task<OrgUnitTypeDescriptor> UpdateAsync(
        Guid id,
        string code,
        string name,
        string icon,
        CancellationToken cancellationToken)
    {
        var db = dbProvider.DbContext;
        var entity = await db.Set<TUnitType>().FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new KeyNotFoundException("Không tìm thấy loại đơn vị.");

        var normalizedCode = NormalizeCode(code);
        if (!string.Equals(entity.Code, normalizedCode, StringComparison.Ordinal))
        {
            var inUse = await db.Set<TUnit>().AnyAsync(
                x => x.OrgId == entity.OrgId && x.UnitType == entity.Code,
                cancellationToken).ConfigureAwait(false);
            if (inUse)
                throw new InvalidOperationException("Không đổi mã khi đã có đơn vị dùng loại này.");

            var duplicate = await db.Set<TUnitType>().AnyAsync(
                x => x.OrgId == entity.OrgId && x.Code == normalizedCode && x.Id != id,
                cancellationToken).ConfigureAwait(false);
            if (duplicate)
                throw new InvalidOperationException("Mã loại đơn vị đã tồn tại trong tổ chức.");
        }

        entity.Code = normalizedCode;
        entity.Name = name.Trim();
        entity.Icon = options.Value.NormalizeIcon(icon);
        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Map(entity);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var db = dbProvider.DbContext;
        var entity = await db.Set<TUnitType>().FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new KeyNotFoundException("Không tìm thấy loại đơn vị.");

        var inUse = await db.Set<TUnit>().AnyAsync(
            x => x.OrgId == entity.OrgId && x.UnitType == entity.Code,
            cancellationToken).ConfigureAwait(false);
        if (inUse)
            throw new InvalidOperationException("Không xóa loại đơn vị đang được gắn trên cây.");

        db.Set<TUnitType>().Remove(entity);
        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    private async Task SeedDefaultsAsync(DbContext db, Guid orgId, CancellationToken cancellationToken)
    {
        var sort = 0;
        foreach (var item in options.Value.UnitTypes)
        {
            var seed = Activator.CreateInstance<TUnitType>();
            seed.Id = Guid.NewGuid();
            seed.OrgId = orgId;
            seed.Code = item.Code;
            seed.Name = item.Name;
            seed.Icon = item.Icon;
            seed.SortOrder = sort++;
            db.Set<TUnitType>().Add(seed);
        }

        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    private static string NormalizeCode(string code)
    {
        var value = code.Trim();
        if (value.Length == 0 || value.Length > 64)
            throw new ArgumentException("Mã loại đơn vị không hợp lệ.");

        return value;
    }

    private static OrgUnitTypeDescriptor Map(TUnitType entity) =>
        new(entity.Id, entity.Code, entity.Name, entity.Icon, entity.SortOrder);
}
