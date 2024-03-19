namespace Jarvis.Domain.Common.V1;

public class Paged<T>
{
    public Paged(int totalRecords, IEnumerable<T> entities)
    {
        TotalRecords = totalRecords;
        Entities = entities;
    }

    public int TotalRecords { get; set; } = 0;
    public IEnumerable<T> Entities { get; set; } = Enumerable.Empty<T>();
}