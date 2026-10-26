namespace Platform.Tenants.Persistence;

/// <summary>Persistence catalog tenant trên Master DB. Không chuyển DbContext sang DB của tenant đang ghi.</summary>
public interface ITenantStore<TTenant>
    where TTenant : class, ITenantCatalogEntity
{
    Task<TTenant?> FindTrackedAsync(Guid id, CancellationToken cancellationToken);

    Task<TTenant?> FindByCodeAsync(string code, CancellationToken cancellationToken);

    Task<bool> CodeExistsAsync(string code, Guid? exceptId, CancellationToken cancellationToken);

    Task<IReadOnlyList<TTenant>> ListAllAsync(CancellationToken cancellationToken);

    Task AddAsync(TTenant entity, CancellationToken cancellationToken);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
