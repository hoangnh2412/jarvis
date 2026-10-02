namespace Platform.DDD.Domain.Entities;

public interface IConcurrencyCheck
{
    public byte[] RowVersion { get; set; }
}