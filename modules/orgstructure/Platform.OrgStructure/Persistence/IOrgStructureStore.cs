namespace Platform.OrgStructure.Persistence;

public interface IOrgStructureStore<TUnit, TLegalEntity, TJobTitle, TPosition>
    where TUnit : class, IOrganizationalUnitEntity
    where TLegalEntity : class, ILegalEntityEntity
    where TJobTitle : class, IJobTitleEntity
    where TPosition : class, IPositionEntity
{
    Task<TUnit?> FindUnitAsync(Guid id, CancellationToken cancellationToken);

    Task<IReadOnlyList<TUnit>> ListUnitsAsync(Guid orgId, CancellationToken cancellationToken);

    Task AddUnitAsync(TUnit entity, CancellationToken cancellationToken);

    Task<TLegalEntity?> FindLegalEntityAsync(Guid id, CancellationToken cancellationToken);

    Task<IReadOnlyList<TLegalEntity>> ListLegalEntitiesAsync(Guid orgId, CancellationToken cancellationToken);

    Task AddLegalEntityAsync(TLegalEntity entity, CancellationToken cancellationToken);

    Task<TJobTitle?> FindJobTitleAsync(Guid id, CancellationToken cancellationToken);

    Task<IReadOnlyList<TJobTitle>> ListJobTitlesAsync(Guid orgId, CancellationToken cancellationToken);

    Task AddJobTitleAsync(TJobTitle entity, CancellationToken cancellationToken);

    Task<TPosition?> FindPositionAsync(Guid id, CancellationToken cancellationToken);

    Task<IReadOnlyList<TPosition>> ListPositionsAsync(Guid orgId, CancellationToken cancellationToken);

    Task AddPositionAsync(TPosition entity, CancellationToken cancellationToken);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
