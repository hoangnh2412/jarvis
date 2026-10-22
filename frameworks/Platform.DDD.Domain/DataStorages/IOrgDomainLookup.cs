namespace Platform.DDD.Domain.DataStorages;

/// <summary>Host map HTTP host → org (custom domain per org).</summary>
public interface IOrgDomainLookup
{
    Task<Guid?> ResolveOrgIdByHostAsync(string host, CancellationToken cancellationToken = default);
}
