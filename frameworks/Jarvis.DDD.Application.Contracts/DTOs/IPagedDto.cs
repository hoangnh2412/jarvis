namespace Jarvis.DDD.Application.Contracts.DTOs;

/// <summary>
/// The interface abstracts input pagination parameters
/// </summary>
/// <typeparam name="T"></typeparam>
public interface IPagedDto<T>
{
    /// <summary>
    /// Number of records returned
    /// </summary>
    /// <value></value>
    int Size { get; set; }

    /// <summary>
    /// Total number of records of the query result
    /// </summary>
    /// <value></value>
    int TotalItems { get; set; }

    /// <summary>
    /// Current page (1-based)
    /// </summary>
    /// <value></value>
    int Page { get; set; }

    /// <summary>
    /// Total number of pages
    /// </summary>
    /// <value></value>
    int TotalPages { get; set; }

    /// <summary>
    /// List of records
    /// </summary>
    /// <value></value>
    IEnumerable<T> Data { get; set; }
}