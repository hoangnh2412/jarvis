namespace Infrastructure.Database.Abstractions
{
    public interface ILogDeletedVersionEntity<T>
    {
        T DeletedVersion { get; set; }
    }
}
