using Jarvis.Domain.Common;

namespace Sample.RequestResponseModels;
public class CustomPaged<T>
{
    public virtual int Size { get; set; } = 10;
    public virtual int TotalItems { get; set; } = 0;
    public virtual int CurrentPage { get; set; } = 1;
    public virtual int TotalPages { get; set; } = 0;
    public virtual string Query { get; set; }
    public virtual IEnumerable<T> Data { get; set; } = Enumerable.Empty<T>();

    public static implicit operator CustomPaged<T>(Paged<T> input)
    {
        return new CustomPaged<T>
        {
            Data = input.Data,
            CurrentPage = input.Page,
            Query = input.Query,
            Size = input.Size,
            TotalItems = input.TotalItems,
            TotalPages = input.TotalPages
        };
    }
}