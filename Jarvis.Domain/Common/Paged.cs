using Jarvis.Domain.Common.Interfaces;

namespace Jarvis.Domain.Common;

public class Paged<T> : IPaged<T>
{
    public int Size { get; set; } = 10;
    public int TotalItems { get; set; } = 0;
    public int Page { get; set; } = 1;
    public int TotalPages { get; set; } = 0;
    public string Query { get; set; }
    public IEnumerable<T> Data { get; set; } = Enumerable.Empty<T>();
}