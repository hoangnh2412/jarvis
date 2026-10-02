namespace Platform.DDD.Domain.Entities;

public interface ITenantEntity
{
    Guid TenantId { get; set; }
}