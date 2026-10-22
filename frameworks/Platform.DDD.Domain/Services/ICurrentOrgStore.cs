namespace Platform.DDD.Domain.Services;

/// <summary>Load profile org (catalog). Host hoặc module Orgs đăng ký implementation.</summary>
public interface ICurrentOrgStore<TOrg>
    where TOrg : class, ICurrentOrgIdentity
{
    Task<TOrg?> FindAsync(Guid orgId, CancellationToken cancellationToken = default);
}
