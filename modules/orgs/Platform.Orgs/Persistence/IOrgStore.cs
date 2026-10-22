namespace Platform.Orgs.Persistence;

/// <summary>Persistence nội bộ catalog. EF đăng ký implementation; test thay bằng store bộ nhớ.</summary>
public interface IOrgStore<TOrg>
    where TOrg : class, IOrgCatalogEntity
{
    Task<TOrg?> FindTrackedAsync(Guid tenantId, Guid id, CancellationToken cancellationToken);

    Task<TOrg?> FindByCodeAsync(Guid tenantId, string code, CancellationToken cancellationToken);

    Task<bool> CodeExistsAsync(Guid tenantId, string code, Guid? exceptId, CancellationToken cancellationToken);

    Task<IReadOnlyList<TOrg>> ListAsync(
        Guid tenantId,
        bool? isActive,
        string? search,
        CancellationToken cancellationToken);

    Task AddAsync(TOrg entity, CancellationToken cancellationToken);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
