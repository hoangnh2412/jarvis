using Microsoft.AspNetCore.Http;
using Platform.DDD.Domain.DataStorages;
using Platform.DDD.Domain.Services;
using Platform.MultiOrg.DataStorages;

namespace Platform.MultiOrg.Scope;

/// <inheritdoc cref="IRequestScopeResolver"/>
public sealed class OrgFirstRequestScopeResolver(
    IHttpContextAccessor httpContextAccessor,
    ICurrentOrgAccessor currentOrgAccessor,
    ICurrentTenantAccessor currentTenantAccessor,
    ITenantIdResolverFactory tenantIdResolverFactory,
    IOrgTenantScopeValidator orgTenantScopeValidator,
    ITenantDomainLookup? tenantDomainLookup = null) : IRequestScopeResolver
{
    public const string TenantHeaderName = "X-Tenant-Id";

    private RequestScopeResolution? _cached;

    public async Task<RequestScopeResolution> ResolveAsync(CancellationToken cancellationToken = default)
    {
        if (_cached is { } cached)
            return cached;

        cancellationToken.ThrowIfCancellationRequested();

        if (currentOrgAccessor.OrgId is Guid ambientOrg
            && currentTenantAccessor.TenantId is Guid ambientTenant
            && ambientOrg != Guid.Empty
            && ambientTenant != Guid.Empty)
        {
            _cached = RequestScopeResolution.Allowed(ambientOrg, ambientTenant);
            return _cached.Value;
        }

        var ctx = httpContextAccessor.HttpContext;
        if (ctx is null)
        {
            _cached = RequestScopeResolution.Forbidden;
            return _cached.Value;
        }

        var hasTenantHeader = HasHeader(ctx, TenantHeaderName);
        var hasOrgHeader = HasHeader(ctx, HeaderOrgIdResolver.HeaderName);

        if (!hasTenantHeader && !hasOrgHeader)
        {
            _cached = RequestScopeResolution.Forbidden;
            return _cached.Value;
        }

        if (hasTenantHeader && !hasOrgHeader)
        {
            _cached = RequestScopeResolution.Forbidden;
            return _cached.Value;
        }

        var orgId = ParseHeaderGuid(ctx, HeaderOrgIdResolver.HeaderName);
        if (!orgId.HasValue)
        {
            _cached = RequestScopeResolution.Forbidden;
            return _cached.Value;
        }

        Guid? tenantId;
        if (!hasTenantHeader)
            tenantId = await ResolveTenantWithoutHeaderAsync(ctx, cancellationToken).ConfigureAwait(false);
        else
            tenantId = ParseHeaderGuid(ctx, TenantHeaderName);

        if (!tenantId.HasValue || tenantId == Guid.Empty)
        {
            _cached = RequestScopeResolution.Forbidden;
            return _cached.Value;
        }

        // Cache trước validate để UoW/OrgStore không gọi lại ResolveAsync đệ quy khi mở DB.
        _cached = RequestScopeResolution.Allowed(orgId.Value, tenantId.Value);

        try
        {
            await orgTenantScopeValidator
                .ValidateOrgBelongsToTenantAsync(orgId.Value, tenantId.Value, cancellationToken)
                .ConfigureAwait(false);
        }
        catch
        {
            _cached = RequestScopeResolution.Forbidden;
            return _cached.Value;
        }

        return _cached.Value;
    }

    private async Task<Guid?> ResolveTenantWithoutHeaderAsync(HttpContext ctx, CancellationToken cancellationToken)
    {
        var host = ctx.Request.Host.Host;
        if (!string.IsNullOrWhiteSpace(host) && tenantDomainLookup is not null)
        {
            var fromDomain = await tenantDomainLookup.ResolveTenantIdByHostAsync(host, cancellationToken)
                .ConfigureAwait(false);
            if (fromDomain.HasValue)
                return fromDomain;
        }

        return await tenantIdResolverFactory.GetTenantIdForOrgScopeAsync(cancellationToken).ConfigureAwait(false);
    }

    private static bool HasHeader(HttpContext ctx, string name) =>
        ctx.Request.Headers.TryGetValue(name, out var value) && !string.IsNullOrWhiteSpace(value.ToString());

    private static Guid? ParseHeaderGuid(HttpContext ctx, string headerName)
    {
        if (!ctx.Request.Headers.TryGetValue(headerName, out var raw) || string.IsNullOrWhiteSpace(raw.ToString()))
            return null;

        return Guid.TryParse(raw.ToString(), out var id) ? id : null;
    }
}
