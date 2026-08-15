// Represents the unified DTO for list requests containing pagination, filter, sort, and column criteria.
using System.ComponentModel.DataAnnotations;

namespace Jarvis.DDD.Domain.Repositories;

/// <summary>
/// Paging with optional column projection, dynamic filtering, and sorting.
/// </summary>
public sealed class PagedListRequest
{
    /// <summary>One-based page number (first page = 1).</summary>
    [Range(1, int.MaxValue, ErrorMessage = "Page must be greater than or equal to 1.")]
    public int Page { get; init; } = 1;

    /// <summary>Number of items per page.</summary>
    [Range(1, int.MaxValue, ErrorMessage = "Size must be greater than or equal to 1.")]
    public int Size { get; init; } = 10;

    /// <summary>
    /// JSON string representing the filter in a nested array format (e.g., ["Name", "contains", "abc"]).
    /// </summary>
    public string? Filter { get; init; }

    /// <summary>
    /// Comma-separated sort string (e.g., "FirstName:asc,LastName:desc").
    /// </summary>
    public string? Sort { get; init; }

    /// <summary>
    /// Comma-separated property names to project; <c>Id</c> is included automatically when the entity defines it.
    /// Requires a parameterless constructor on the entity type.
    /// </summary>
    public string? Columns { get; init; }
}
