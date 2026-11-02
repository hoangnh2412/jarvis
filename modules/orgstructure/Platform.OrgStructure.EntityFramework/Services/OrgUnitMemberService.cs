using Microsoft.EntityFrameworkCore;
using Platform.Modules.IdentityAdmin.Services;
using Platform.OrgStructure.EntityFramework.Persistence;
using Platform.OrgStructure.Services;

namespace Platform.OrgStructure.EntityFramework.Services;

/// <summary>
/// Thành viên đơn vị: CRUD + đồng bộ claim phạm vi tổ chức của user qua <see cref="RealmUserOperations"/>.
/// Entity do host sở hữu; service thao tác qua <see cref="IOrgStructureDbContextProvider"/>.
/// </summary>
public sealed class OrgUnitMemberService<TMember, TUnit, TJobPosition, TJobGrade, TJobTitle>(
    IOrgStructureDbContextProvider dbProvider,
    RealmUserOperations realmUsers) : IOrgUnitMemberService
    where TMember : class, IOrgUnitMemberEntity
    where TUnit : class, IOrganizationalUnitEntity
    where TJobPosition : class, IOrgNamedCatalogEntity
    where TJobGrade : class, IOrgNamedCatalogEntity
    where TJobTitle : class, IJobTitleEntity
{
    public async Task<IReadOnlyList<OrgUnitMemberCount>> CountByOrgAsync(
        Guid orgId,
        CancellationToken cancellationToken)
    {
        var db = dbProvider.DbContext;
        return await db.Set<TMember>()
            .AsNoTracking()
            .Where(x => x.OrgId == orgId)
            .GroupBy(x => x.OrganizationalUnitId)
            .Select(g => new OrgUnitMemberCount(
                g.Key,
                g.Count(x => x.IsActive),
                g.Count()))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<OrgUnitMemberDto>> ListByUnitAsync(
        Guid orgId,
        Guid organizationalUnitId,
        CancellationToken cancellationToken)
    {
        var db = dbProvider.DbContext;
        await EnsureUnitInOrgAsync(db, orgId, organizationalUnitId, cancellationToken).ConfigureAwait(false);

        var rows = await db.Set<TMember>()
            .AsNoTracking()
            .Where(x => x.OrgId == orgId && x.OrganizationalUnitId == organizationalUnitId)
            .OrderByDescending(x => x.IsActive)
            .ThenBy(x => x.Id)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        if (rows.Count == 0)
            return [];

        var positionIds = rows.Select(x => x.JobPositionId).Distinct().ToArray();
        var gradeIds = rows.Select(x => x.JobGradeId).Distinct().ToArray();
        var titleIds = rows.Where(x => x.JobTitleId.HasValue).Select(x => x.JobTitleId!.Value).Distinct().ToArray();

        var positions = await db.Set<TJobPosition>()
            .AsNoTracking()
            .Where(x => positionIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, cancellationToken)
            .ConfigureAwait(false);
        var grades = await db.Set<TJobGrade>()
            .AsNoTracking()
            .Where(x => gradeIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, cancellationToken)
            .ConfigureAwait(false);
        var titles = titleIds.Length == 0
            ? new Dictionary<Guid, TJobTitle>()
            : await db.Set<TJobTitle>()
                .AsNoTracking()
                .Where(x => titleIds.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, cancellationToken)
                .ConfigureAwait(false);

        var result = new List<OrgUnitMemberDto>(rows.Count);
        foreach (var row in rows)
        {
            var user = await realmUsers.FindByIdAsync(row.UserId, cancellationToken).ConfigureAwait(false);
            string? displayName = user is null
                ? null
                : await realmUsers.GetDisplayNameAsync(user, cancellationToken).ConfigureAwait(false);

            positions.TryGetValue(row.JobPositionId, out var position);
            grades.TryGetValue(row.JobGradeId, out var grade);
            TJobTitle? title = null;
            if (row.JobTitleId is Guid titleId)
                titles.TryGetValue(titleId, out title);

            result.Add(new OrgUnitMemberDto(
                row.Id,
                row.UserId,
                user?.UserName ?? row.UserId.ToString(),
                displayName,
                row.JobPositionId,
                position?.Code ?? string.Empty,
                position?.Name ?? string.Empty,
                row.JobGradeId,
                grade?.Code ?? string.Empty,
                grade?.Name ?? string.Empty,
                row.JobTitleId,
                title?.Code,
                title?.Name,
                row.IsActive,
                row.OrganizationalUnitId));
        }

        return result;
    }

    public async Task<OrgUnitMemberDto> CreateAsync(
        Guid orgId,
        Guid organizationalUnitId,
        OrgUnitMemberCreateInput input,
        CancellationToken cancellationToken)
    {
        var db = dbProvider.DbContext;
        await EnsureUnitInOrgAsync(db, orgId, organizationalUnitId, cancellationToken).ConfigureAwait(false);
        await EnsureCatalogRefsAsync(db, orgId, input.JobPositionId, input.JobGradeId, input.JobTitleId, cancellationToken)
            .ConfigureAwait(false);

        _ = await realmUsers.FindByIdAsync(input.UserId, cancellationToken).ConfigureAwait(false)
            ?? throw new KeyNotFoundException("Không tìm thấy người dùng.");

        var duplicate = await db.Set<TMember>().AnyAsync(
            x => x.OrganizationalUnitId == organizationalUnitId && x.UserId == input.UserId,
            cancellationToken).ConfigureAwait(false);
        if (duplicate)
            throw new InvalidOperationException("Người dùng đã có trong đơn vị này.");

        var entity = Activator.CreateInstance<TMember>();
        entity.Id = Guid.NewGuid();
        entity.OrgId = orgId;
        entity.OrganizationalUnitId = organizationalUnitId;
        entity.UserId = input.UserId;
        entity.JobPositionId = input.JobPositionId;
        entity.JobGradeId = input.JobGradeId;
        entity.JobTitleId = input.JobTitleId;
        entity.IsActive = true;
        db.Set<TMember>().Add(entity);
        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        await SyncClaimsAsync(db, entity, cancellationToken).ConfigureAwait(false);
        var list = await ListByUnitAsync(orgId, organizationalUnitId, cancellationToken).ConfigureAwait(false);
        return list.First(x => x.Id == entity.Id);
    }

    public async Task<OrgUnitMemberDto> UpdateAsync(Guid id, OrgUnitMemberUpdateInput input, CancellationToken cancellationToken)
    {
        var db = dbProvider.DbContext;
        var entity = await db.Set<TMember>().FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new KeyNotFoundException("Không tìm thấy thành viên.");

        await EnsureCatalogRefsAsync(db, entity.OrgId, input.JobPositionId, input.JobGradeId, input.JobTitleId, cancellationToken)
            .ConfigureAwait(false);
        entity.JobPositionId = input.JobPositionId;
        entity.JobGradeId = input.JobGradeId;
        entity.JobTitleId = input.JobTitleId;
        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        await SyncClaimsAsync(db, entity, cancellationToken).ConfigureAwait(false);

        var list = await ListByUnitAsync(entity.OrgId, entity.OrganizationalUnitId, cancellationToken)
            .ConfigureAwait(false);
        return list.First(x => x.Id == id);
    }

    public async Task<OrgUnitMemberDto> SetActiveAsync(Guid id, bool isActive, CancellationToken cancellationToken)
    {
        var db = dbProvider.DbContext;
        var entity = await db.Set<TMember>().FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new KeyNotFoundException("Không tìm thấy thành viên.");
        entity.IsActive = isActive;
        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        if (isActive)
            await SyncClaimsAsync(db, entity, cancellationToken).ConfigureAwait(false);
        else
            await ResyncOrClearClaimsAsync(db, entity.UserId, cancellationToken).ConfigureAwait(false);

        var list = await ListByUnitAsync(entity.OrgId, entity.OrganizationalUnitId, cancellationToken)
            .ConfigureAwait(false);
        return list.First(x => x.Id == id);
    }

    private async Task SyncClaimsAsync(DbContext db, TMember member, CancellationToken cancellationToken)
    {
        var unit = await db.Set<TUnit>()
            .AsNoTracking()
            .FirstAsync(x => x.Id == member.OrganizationalUnitId, cancellationToken)
            .ConfigureAwait(false);
        var user = await realmUsers.FindByIdAsync(member.UserId, cancellationToken).ConfigureAwait(false)
            ?? throw new KeyNotFoundException("Không tìm thấy người dùng.");
        await realmUsers.SyncOrgClaimsAsync(
                user,
                unit.Id,
                unit.UnitType,
                unit.LegalEntityId?.ToString(),
                unit.Path,
                member.JobPositionId,
                cancellationToken)
            .ConfigureAwait(false);
    }

    private async Task ResyncOrClearClaimsAsync(DbContext db, Guid userId, CancellationToken cancellationToken)
    {
        var next = await db.Set<TMember>()
            .AsNoTracking()
            .Where(row => row.UserId == userId && row.IsActive)
            .OrderByDescending(row => row.Id)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);
        var user = await realmUsers.FindByIdAsync(userId, cancellationToken).ConfigureAwait(false)
            ?? throw new KeyNotFoundException("Không tìm thấy người dùng.");
        if (next is null)
        {
            await realmUsers.ClearOrgScopeAsync(user, cancellationToken).ConfigureAwait(false);
            return;
        }

        await SyncClaimsAsync(db, next, cancellationToken).ConfigureAwait(false);
    }

    private async Task EnsureUnitInOrgAsync(DbContext db, Guid orgId, Guid unitId, CancellationToken cancellationToken)
    {
        var exists = await db.Set<TUnit>().AnyAsync(
            x => x.Id == unitId && x.OrgId == orgId,
            cancellationToken).ConfigureAwait(false);
        if (!exists)
            throw new KeyNotFoundException("Không tìm thấy đơn vị.");
    }

    private async Task EnsureCatalogRefsAsync(
        DbContext db,
        Guid orgId,
        Guid jobPositionId,
        Guid jobGradeId,
        Guid? jobTitleId,
        CancellationToken cancellationToken)
    {
        var positionOk = await db.Set<TJobPosition>().AnyAsync(
            x => x.Id == jobPositionId && x.OrgId == orgId,
            cancellationToken).ConfigureAwait(false);
        var gradeOk = await db.Set<TJobGrade>().AnyAsync(
            x => x.Id == jobGradeId && x.OrgId == orgId,
            cancellationToken).ConfigureAwait(false);
        if (!positionOk || !gradeOk)
            throw new InvalidOperationException("Vị trí công việc hoặc cấp chuyên môn không hợp lệ.");

        if (jobTitleId is Guid titleId)
        {
            var titleOk = await db.Set<TJobTitle>().AnyAsync(
                x => x.Id == titleId && x.OrgId == orgId,
                cancellationToken).ConfigureAwait(false);
            if (!titleOk)
                throw new InvalidOperationException("Chức vụ không hợp lệ.");
        }
    }
}
