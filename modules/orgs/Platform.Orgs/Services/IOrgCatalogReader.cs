using Platform.Orgs.Models;

namespace Platform.Orgs.Services;

/// <summary>
/// Đọc catalog → <typeparamref name="TReadModel"/> (mapper host).
/// Overload không <c>tenantId</c>: ambient <c>ICurrentTenant</c>; overload có <c>tenantId</c>: đối xứng <see cref="Abstractions.IOrgCatalog"/>.
/// </summary>
public interface IOrgCatalogReader<TReadModel>
{
    Task<TReadModel?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<TReadModel?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TReadModel>> ListAsync(ListOrgsQuery? query = null, CancellationToken cancellationToken = default);

    Task<TReadModel?> GetAsync(Guid orgId, Guid tenantId, CancellationToken cancellationToken = default);

    Task<TReadModel?> GetByCodeAsync(string code, Guid tenantId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TReadModel>> ListAsync(
        Guid tenantId,
        bool? isActive = null,
        string? search = null,
        CancellationToken cancellationToken = default);
}
