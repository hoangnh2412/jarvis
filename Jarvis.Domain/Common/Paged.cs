using Jarvis.Domain.Common.Interfaces;

namespace Jarvis.Domain.Common;

public class Paged<T> : IPaged<T>
{
    public virtual int Size { get; set; } = 10;
    public virtual int TotalItems { get; set; } = 0;
    public virtual int Page { get; set; } = 1;
    public virtual int TotalPages { get; set; } = 0;
    public virtual string Query { get; set; }
    public virtual IEnumerable<T> Data { get; set; } = Enumerable.Empty<T>();
}