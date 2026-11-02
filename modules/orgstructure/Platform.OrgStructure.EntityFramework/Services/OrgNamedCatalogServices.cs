using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Platform.OrgStructure.EntityFramework.Persistence;
using Platform.OrgStructure.Models;
using Platform.OrgStructure.Services;

namespace Platform.OrgStructure.EntityFramework.Services;

/// <summary>Nền danh mục đặt tên theo Org: list + seed mặc định khi trống.</summary>
public abstract class OrgNamedCatalogService<TEntity>(
    IOrgStructureDbContextProvider dbProvider,
    IOptions<OrgStructureCatalogOptions> options)
    where TEntity : class, IOrgNamedCatalogEntity
{
    protected abstract IReadOnlyList<OrgCatalogDefault> Defaults(OrgStructureCatalogOptions options);

    public async Task<IReadOnlyList<OrgNamedCatalogDescriptor>> ListAsync(Guid orgId, CancellationToken cancellationToken)
    {
        var db = dbProvider.DbContext;
        var rows = await QueryAsync(db, orgId, cancellationToken).ConfigureAwait(false);
        if (rows.Count > 0)
            return rows;

        var sort = 0;
        foreach (var item in Defaults(options.Value))
        {
            var entity = Activator.CreateInstance<TEntity>();
            entity.Id = Guid.NewGuid();
            entity.OrgId = orgId;
            entity.Code = item.Code;
            entity.Name = item.Name;
            entity.SortOrder = sort++;
            db.Set<TEntity>().Add(entity);
        }

        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return await QueryAsync(db, orgId, cancellationToken).ConfigureAwait(false);
    }

    private static async Task<IReadOnlyList<OrgNamedCatalogDescriptor>> QueryAsync(
        DbContext db,
        Guid orgId,
        CancellationToken cancellationToken)
    {
        var rows = await db.Set<TEntity>()
            .AsNoTracking()
            .Where(x => x.OrgId == orgId)
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.Name)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        return rows.Select(x => new OrgNamedCatalogDescriptor(x.Id, x.Code, x.Name, x.SortOrder)).ToList();
    }
}

public sealed class JobGradeService<TJobGrade>(
    IOrgStructureDbContextProvider dbProvider,
    IOptions<OrgStructureCatalogOptions> options)
    : OrgNamedCatalogService<TJobGrade>(dbProvider, options), IJobGradeService
    where TJobGrade : class, IOrgNamedCatalogEntity
{
    protected override IReadOnlyList<OrgCatalogDefault> Defaults(OrgStructureCatalogOptions options) => options.JobGrades;
}

public sealed class JobPositionService<TJobPosition>(
    IOrgStructureDbContextProvider dbProvider,
    IOptions<OrgStructureCatalogOptions> options)
    : OrgNamedCatalogService<TJobPosition>(dbProvider, options), IJobPositionService
    where TJobPosition : class, IOrgNamedCatalogEntity
{
    protected override IReadOnlyList<OrgCatalogDefault> Defaults(OrgStructureCatalogOptions options) => options.JobPositions;
}

/// <summary>List chức vụ qua <see cref="IOrgStructureManager"/>; seed mặc định qua manager khi Org chưa có chức vụ.</summary>
public sealed class JobTitleCatalogService<TJobTitle>(
    IOrgStructureDbContextProvider dbProvider,
    IOrgStructureManager structure,
    IOptions<OrgStructureCatalogOptions> options) : IJobTitleCatalogService
    where TJobTitle : class, IJobTitleEntity
{
    public async Task<IReadOnlyList<JobTitleDescriptor>> ListAsync(Guid orgId, CancellationToken cancellationToken)
    {
        var db = dbProvider.DbContext;
        var hasAny = await db.Set<TJobTitle>().AnyAsync(x => x.OrgId == orgId, cancellationToken).ConfigureAwait(false);
        if (!hasAny)
        {
            foreach (var item in options.Value.JobTitles)
            {
                await structure.CreateJobTitleAsync(
                    new CreateJobTitleInput(orgId, item.Code, item.Name),
                    cancellationToken).ConfigureAwait(false);
            }
        }

        return await structure.ListJobTitlesAsync(orgId, cancellationToken).ConfigureAwait(false);
    }
}
