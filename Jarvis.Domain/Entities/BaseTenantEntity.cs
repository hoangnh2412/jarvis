namespace Jarvis.Domain.Entities;

public class BaseTenantEntity : BaseEntity<Guid>, ITenantManagementEntity
{
    public required string ConnectionString { get; set; } = string.Empty;
}