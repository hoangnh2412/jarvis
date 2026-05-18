namespace Jarvis.Domain.Entities;

public interface ITenantManagementEntity : IEntity<Guid>
{
    public string ConnectionString { get; set; }
}