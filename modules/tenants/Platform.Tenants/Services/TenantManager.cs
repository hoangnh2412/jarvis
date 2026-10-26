using Platform.DDD.Domain.Shared.ExceptionHandling;
using Platform.Tenants.Models;
using Platform.Tenants.Persistence;

namespace Platform.Tenants.Services;

public sealed class TenantManager<TTenant> : ITenantManager
    where TTenant : class, ITenantCatalogEntity
{
    private readonly ITenantStore<TTenant> _store;

    public TenantManager(ITenantStore<TTenant> store)
    {
        _store = store;
    }

    public async Task<TenantDescriptor> CreateAsync(CreateTenantInput input, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(input);

        var code = NormalizeCode(input.Code);
        var name = NormalizeRequired(input.Name, TenantCatalogLimits.NameMaxLength);
        var connectionString = NormalizeConnectionString(input.ConnectionString);

        if (await _store.CodeExistsAsync(code, null, cancellationToken).ConfigureAwait(false))
            throw new ConflictException(TenantErrorCode.CodeAlreadyExists);

        var id = Guid.NewGuid();
        await EnsureParentAsync(id, input.ParentTenantId, cancellationToken).ConfigureAwait(false);

        var entity = CreateEntity(id);
        entity.Code = code;
        entity.Name = name;
        entity.IsActive = true;
        entity.ParentTenantId = input.ParentTenantId;
        entity.ConnectionString = connectionString;

        await _store.AddAsync(entity, cancellationToken).ConfigureAwait(false);
        await _store.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return ToDescriptor(entity, includeConnectionString: true);
    }

    public async Task<TenantDescriptor> UpdateAsync(
        Guid id,
        UpdateTenantInput input,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(input);

        var entity = await FindRequiredAsync(id, cancellationToken).ConfigureAwait(false);
        var code = NormalizeCode(input.Code);
        var name = NormalizeRequired(input.Name, TenantCatalogLimits.NameMaxLength);
        var connectionString = NormalizeConnectionString(input.ConnectionString);

        if (await _store.CodeExistsAsync(code, id, cancellationToken).ConfigureAwait(false))
            throw new ConflictException(TenantErrorCode.CodeAlreadyExists);

        entity.Code = code;
        entity.Name = name;
        entity.ConnectionString = connectionString;
        await _store.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return ToDescriptor(entity, includeConnectionString: true);
    }

    public async Task<TenantDescriptor> SetActiveAsync(Guid id, bool isActive, CancellationToken cancellationToken = default)
    {
        var entity = await FindRequiredAsync(id, cancellationToken).ConfigureAwait(false);
        entity.IsActive = isActive;
        await _store.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return ToDescriptor(entity, includeConnectionString: true);
    }

    public async Task<TenantDescriptor> SetParentAsync(
        Guid id,
        Guid? parentTenantId,
        CancellationToken cancellationToken = default)
    {
        var entity = await FindRequiredAsync(id, cancellationToken).ConfigureAwait(false);
        await EnsureParentAsync(id, parentTenantId, cancellationToken).ConfigureAwait(false);
        entity.ParentTenantId = parentTenantId;
        await _store.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return ToDescriptor(entity, includeConnectionString: true);
    }

    public async Task<TenantDescriptor?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _store.FindTrackedAsync(id, cancellationToken).ConfigureAwait(false);
        return entity is null ? null : ToDescriptor(entity, includeConnectionString: true);
    }

    public async Task<TenantDescriptor?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        var normalized = NormalizeCode(code);
        var entity = await _store.FindByCodeAsync(normalized, cancellationToken).ConfigureAwait(false);
        return entity is null ? null : ToDescriptor(entity, includeConnectionString: true);
    }

    public async Task<string> GetConnectionStringAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await FindRequiredAsync(id, cancellationToken).ConfigureAwait(false);
        if (!entity.IsActive)
            throw new ConflictException(TenantErrorCode.Inactive);

        return entity.ConnectionString;
    }

    public async Task<IReadOnlyList<TenantDescriptor>> ListAsync(
        ListTenantsQuery? query = null,
        CancellationToken cancellationToken = default)
    {
        var all = await _store.ListAllAsync(cancellationToken).ConfigureAwait(false);
        return Filter(all, query).Select(entity => ToDescriptor(entity, includeConnectionString: false)).ToArray();
    }

    public async Task<IReadOnlyList<TenantDescriptor>> ListSubtreeAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        var all = await _store.ListAllAsync(cancellationToken).ConfigureAwait(false);
        var byId = all.ToDictionary(entity => entity.Id);
        if (!byId.ContainsKey(tenantId))
            throw new NotFoundException(TenantErrorCode.NotFound);

        var children = all.ToLookup(entity => entity.ParentTenantId);
        var ids = new List<Guid>();
        var seen = new HashSet<Guid>();
        var queue = new Queue<Guid>();
        queue.Enqueue(tenantId);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            if (!seen.Add(current) || !byId.ContainsKey(current))
                continue;

            ids.Add(current);
            foreach (var child in children[current])
                queue.Enqueue(child.Id);
        }

        return ids.Select(id => ToDescriptor(byId[id], includeConnectionString: false)).ToArray();
    }

    private async Task EnsureParentAsync(Guid id, Guid? parentTenantId, CancellationToken cancellationToken)
    {
        if (parentTenantId is null)
            return;

        if (parentTenantId == Guid.Empty || parentTenantId == id)
            throw new BadRequestException(TenantErrorCode.InvalidParent);

        var cursor = parentTenantId;
        var guard = 0;
        while (cursor is Guid parentId)
        {
            if (parentId == id || ++guard > 64)
                throw new BadRequestException(TenantErrorCode.InvalidParent);

            var parent = await _store.FindTrackedAsync(parentId, cancellationToken).ConfigureAwait(false);
            if (parent is null)
                throw new NotFoundException(TenantErrorCode.NotFound);

            if (!parent.IsActive)
                throw new ConflictException(TenantErrorCode.ParentInactive);

            cursor = parent.ParentTenantId;
        }
    }

    private async Task<TTenant> FindRequiredAsync(Guid id, CancellationToken cancellationToken)
    {
        var entity = await _store.FindTrackedAsync(id, cancellationToken).ConfigureAwait(false);
        if (entity is null)
            throw new NotFoundException(TenantErrorCode.NotFound);

        return entity;
    }

    private static IEnumerable<TTenant> Filter(IReadOnlyList<TTenant> source, ListTenantsQuery? query)
    {
        IEnumerable<TTenant> rows = source;
        if (query?.IsActive is bool active)
            rows = rows.Where(entity => entity.IsActive == active);

        if (!string.IsNullOrWhiteSpace(query?.Search))
        {
            var term = query.Search.Trim();
            rows = rows.Where(entity =>
                entity.Code.Contains(term, StringComparison.OrdinalIgnoreCase)
                || entity.Name.Contains(term, StringComparison.OrdinalIgnoreCase));
        }

        return rows.OrderBy(entity => entity.Code, StringComparer.Ordinal);
    }

    private static TTenant CreateEntity(Guid id)
    {
        var entity = (TTenant)Activator.CreateInstance(typeof(TTenant))!;
        entity.Id = id;
        return entity;
    }

    private static TenantDescriptor ToDescriptor(TTenant entity, bool includeConnectionString) =>
        new()
        {
            Id = entity.Id,
            Code = entity.Code,
            Name = entity.Name,
            IsActive = entity.IsActive,
            ParentTenantId = entity.ParentTenantId,
            ConnectionString = includeConnectionString ? entity.ConnectionString : null,
        };

    private static string NormalizeCode(string? value)
    {
        var trimmed = value?.Trim() ?? string.Empty;
        if (trimmed.Length == 0)
            throw new BadRequestException(TenantErrorCode.InvalidInput);

        var normalized = TenantCatalogLimits.NormalizeCode(trimmed);
        if (normalized.Length > TenantCatalogLimits.CodeMaxLength)
            throw new BadRequestException(TenantErrorCode.InvalidInput);

        return normalized;
    }

    private static string NormalizeRequired(string? value, int maxLength)
    {
        var trimmed = value?.Trim() ?? string.Empty;
        if (trimmed.Length == 0 || trimmed.Length > maxLength)
            throw new BadRequestException(TenantErrorCode.InvalidInput);

        return trimmed;
    }

    private static string NormalizeConnectionString(string? value)
    {
        var trimmed = value?.Trim() ?? string.Empty;
        if (trimmed.Length == 0 || trimmed.Length > TenantCatalogLimits.ConnectionStringMaxLength)
            throw new BadRequestException(TenantErrorCode.InvalidInput);

        return trimmed;
    }
}
