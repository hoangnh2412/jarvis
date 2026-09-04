namespace Jarvis.DDD.Application.Contracts.DTOs;

/// <summary>
/// The interface abstracts output pagination parameters
/// </summary>
public interface IPagingDto
{
    /// <summary>
    /// Number of records returned
    /// </summary>
    /// <value></value>
    int Size { get; set; }

    /// <summary>
    /// Current page (1-based)
    /// </summary>
    /// <value></value>
    int Page { get; set; }

    /// <summary>
    /// JSON string representing the filter in a nested array format
    /// </summary>
    string? Filter { get; set; }

    /// <summary>
    /// Comma-separated sort string
    /// </summary>
    string? Sort { get; set; }

    /// <summary>
    /// Comma-separated fields to return
    /// </summary>
    string? Columns { get; set; }
}