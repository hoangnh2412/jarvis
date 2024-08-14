namespace Jarvis.Domain.Common.Interfaces;

/// <summary>
/// The interface abstracts input pagination parameters
/// </summary>
/// <typeparam name="T"></typeparam>
public interface IPaged<T>
{
    int Size { get; set; }
    int TotalItems { get; set; }
    int Page { get; set; }
    int TotalPages { get; set; }
    string Query { get; set; }
    IEnumerable<T> Data { get; set; }
}