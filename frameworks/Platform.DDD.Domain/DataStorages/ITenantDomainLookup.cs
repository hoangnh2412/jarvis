namespace Platform.DDD.Domain.DataStorages;

/// <summary>Host map HTTP host → tenant khi host không thuộc org domain.</summary>
public interface ITenantDomainLookup
{
    Task<Guid?> ResolveTenantIdByHostAsync(string host, CancellationToken cancellationToken = default);
}
