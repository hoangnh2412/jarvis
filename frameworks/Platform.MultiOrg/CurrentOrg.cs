using Platform.DDD.Domain.DataStorages;
using Platform.DDD.Domain.Services;

namespace Platform.MultiOrg;

/// <summary>API org công khai — <see cref="GetIdAsync"/> (R2) và <see cref="GetAsync"/> (store).</summary>
public sealed class CurrentOrg<TOrg>(
    ICurrentOrgAccessor currentOrgAccessor,
    IOrgIdResolverFactory orgIdResolverFactory,
    ICurrentOrgStore<TOrg> orgStore,
    IRequestScopeResolver? requestScopeResolver = null) : ICurrentOrg<TOrg>
    where TOrg : class, ICurrentOrgIdentity
{
    private Guid? _resolvedOrgId;
    private bool _resolved;

    private Guid? _cachedProfileId;
    private TOrg? _profile;
    private bool _profileLoaded;

    public async Task<Guid?> GetIdAsync(CancellationToken cancellationToken = default)
    {
        var ambient = currentOrgAccessor.OrgId;
        if (ambient.HasValue)
            return ambient;

        if (_resolved)
            return _resolvedOrgId;

        if (requestScopeResolver is not null)
        {
            var scope = await requestScopeResolver.ResolveAsync(cancellationToken).ConfigureAwait(false);
            _resolvedOrgId = scope.IsAllowed ? scope.OrgId : null;
        }
        else
        {
            _resolvedOrgId = await orgIdResolverFactory
                .GetOrgIdAsync(cancellationToken)
                .ConfigureAwait(false);
        }
        _resolved = true;
        return _resolvedOrgId;
    }

    public async Task<TOrg?> GetAsync(CancellationToken cancellationToken = default)
    {
        var orgId = await GetIdAsync(cancellationToken).ConfigureAwait(false);
        if (!orgId.HasValue)
        {
            _profileLoaded = true;
            _cachedProfileId = null;
            _profile = null;
            return null;
        }

        if (_profileLoaded && _cachedProfileId == orgId)
            return _profile;

        _profile = await orgStore
            .FindAsync(orgId.Value, cancellationToken)
            .ConfigureAwait(false);
        _cachedProfileId = orgId;
        _profileLoaded = true;
        return _profile;
    }

    public IDisposable Change(Guid orgId)
    {
        if (orgId == Guid.Empty)
            throw new ArgumentException("OrgId is required.", nameof(orgId));

        return currentOrgAccessor.BeginScope(orgId);
    }
}
