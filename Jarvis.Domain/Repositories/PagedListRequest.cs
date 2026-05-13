namespace Jarvis.Domain.Repositories;

/// <summary>
/// Paging with optional column projection (see <see cref="Columns"/>).
/// </summary>
public sealed class PagedListRequest
{
    /// <summary>Zero-based page index (first page = 0).</summary>
    public int PageIndex { get; init; }

    public int PageSize { get; init; }

    /// <summary>
    /// Comma-separated property names to project; <c>Id</c> is included automatically when the entity defines it.
    /// Requires a parameterless constructor on the entity type.
    /// </summary>
    public string? Columns { get; init; }
}
