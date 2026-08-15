using Jarvis.DDD.Domain.Entities;

namespace Jarvis.DDD.Domain.Repositories;

public sealed class PagedQueryOptions<TEntity> where TEntity : class, IEntity
{
    private ISet<string>? _allowedFields;

    /// <summary>
    /// Whitelist of allowed field names for dynamic filtering and sorting.
    /// If null or empty, ALL dynamic filtering and sorting is blocked for security.
    /// Values are always compared case-insensitively.
    /// </summary>
    public ISet<string>? AllowedFields
    {
        get => _allowedFields;
        init => _allowedFields = value != null
            ? new HashSet<string>(value, StringComparer.OrdinalIgnoreCase)
            : null;
    }

    private ISet<string>? _deniedFields;

    /// <summary>
    /// Blacklist of explicitly denied field names. 
    /// Takes precedence over AllowedFields. If a field is in this list, it is blocked.
    /// Values are always compared case-insensitively.
    /// </summary>
    public ISet<string>? DeniedFields
    {
        get => _deniedFields;
        init => _deniedFields = value != null
            ? new HashSet<string>(value, StringComparer.OrdinalIgnoreCase)
            : null;
    }

    /// <summary>
    /// Overrides the dynamic filter parser completely. If set, the dynamic filter string is ignored.
    /// </summary>
    public Func<IQueryable<TEntity>, IQueryable<TEntity>>? CustomFilter { get; init; }

    /// <summary>
    /// Overrides the dynamic sort parser completely. If set, the dynamic sort string is ignored.
    /// </summary>
    public Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? CustomSort { get; init; }
}
