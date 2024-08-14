namespace Jarvis.Application.Contracts.DTOs;

/// <summary>
/// The interface abstracts output pagination parameters
/// </summary>
public interface IPagingDto
{
    /// <summary>
    /// Number of records returned
    /// </summary>
    /// <value></value>
    int PageSize { get; set; }

    /// <summary>
    /// Current page
    /// </summary>
    /// <value></value>
    int PageIndex { get; set; }

    /// <summary>
    /// List of fields to filter
    /// Key = Field name
    /// Value = Value to filter
    /// </summary>
    Dictionary<string, string> Filters { get; set; }

    /// <summary>
    /// List of fields to sort. The sorting order will follow this list
    /// Key = Field name
    /// Value = asc/desc
    /// </summary>
    Dictionary<string, string> Sort { get; set; }

    /// <summary>
    /// List of fields to returned
    /// Ex: field1,field2
    /// </summary>
    IEnumerable<string> Columns { get; set; }
}