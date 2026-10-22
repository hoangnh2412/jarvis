using Microsoft.AspNetCore.Http;
using Platform.DDD.Domain.DataStorages;
using Platform.DDD.Domain.Services;
using Platform.MultiOrg;
using Platform.MultiOrg.DataStorages;
using Platform.MultiOrg.Scope;
using Platform.Multitenancy;

namespace UnitTest.MultiOrg;

public sealed class OrgFirstScopeResolverTests
{
    private static readonly Guid OrgHeader = Guid.Parse("22222222-2222-2222-2222-222222222222");
    private static readonly Guid TenantDomain = Guid.Parse("33333333-3333-3333-3333-333333333333");
    private static readonly Guid TenantHeader = Guid.Parse("44444444-4444-4444-4444-444444444444");

    [Fact]
    public async Task Rule1_No_Headers_Forbidden_Even_With_Tenant_Domain()
    {
        var ctx = new DefaultHttpContext();
        ctx.Request.Host = new HostString("tenant.example.com");

        var resolver = CreateResolver(ctx, tenantDomain: new FixedTenantDomainLookup(TenantDomain));
        var scope = await resolver.ResolveAsync();

        Assert.True(scope.IsForbidden);
    }

    [Fact]
    public async Task Rule3_Tenant_Header_Without_Org_Forbidden()
    {
        var ctx = new DefaultHttpContext();
        ctx.Request.Headers[OrgFirstRequestScopeResolver.TenantHeaderName] = TenantHeader.ToString();

        var scope = await CreateResolver(ctx).ResolveAsync();

        Assert.True(scope.IsForbidden);
    }

    [Fact]
    public async Task Rule2_Org_Header_Tenant_From_Domain()
    {
        var ctx = new DefaultHttpContext();
        ctx.Request.Host = new HostString("tenant.example.com");
        ctx.Request.Headers[HeaderOrgIdResolver.HeaderName] = OrgHeader.ToString();

        var scope = await CreateResolver(
            ctx,
            tenantDomain: new FixedTenantDomainLookup(TenantDomain)).ResolveAsync();

        Assert.True(scope.IsAllowed);
        Assert.Equal(OrgHeader, scope.OrgId);
        Assert.Equal(TenantDomain, scope.TenantId);
    }

    [Fact]
    public async Task Rule4_Both_Headers()
    {
        var ctx = new DefaultHttpContext();
        ctx.Request.Headers[HeaderOrgIdResolver.HeaderName] = OrgHeader.ToString();
        ctx.Request.Headers[OrgFirstRequestScopeResolver.TenantHeaderName] = TenantHeader.ToString();

        var scope = await CreateResolver(ctx).ResolveAsync();

        Assert.True(scope.IsAllowed);
        Assert.Equal(OrgHeader, scope.OrgId);
        Assert.Equal(TenantHeader, scope.TenantId);
    }

    [Fact]
    public async Task Validation_Failure_Forbidden()
    {
        var ctx = new DefaultHttpContext();
        ctx.Request.Headers[HeaderOrgIdResolver.HeaderName] = OrgHeader.ToString();
        ctx.Request.Headers[OrgFirstRequestScopeResolver.TenantHeaderName] = TenantHeader.ToString();

        var scope = await CreateResolver(ctx, validator: new ThrowingValidator()).ResolveAsync();

        Assert.True(scope.IsForbidden);
    }

    private static OrgFirstRequestScopeResolver CreateResolver(
        HttpContext httpContext,
        ITenantDomainLookup? tenantDomain = null,
        IOrgTenantScopeValidator? validator = null,
        ITenantIdResolverFactory? tenantFactory = null)
    {
        return new OrgFirstRequestScopeResolver(
            new HttpContextAccessor { HttpContext = httpContext },
            new CurrentOrgAccessor(),
            new CurrentTenantAccessor(),
            tenantFactory ?? new StubTenantIdResolverFactory(),
            validator ?? new NoOpValidator(),
            tenantDomain);
    }

    private sealed class NoOpValidator : IOrgTenantScopeValidator
    {
        public Task ValidateOrgBelongsToTenantAsync(Guid orgId, Guid tenantId, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;
    }

    private sealed class ThrowingValidator : IOrgTenantScopeValidator
    {
        public Task ValidateOrgBelongsToTenantAsync(Guid orgId, Guid tenantId, CancellationToken cancellationToken = default) =>
            throw new InvalidOperationException("not in tenant");
    }

    private sealed class FixedTenantDomainLookup(Guid? tenantId) : ITenantDomainLookup
    {
        public Task<Guid?> ResolveTenantIdByHostAsync(string host, CancellationToken cancellationToken = default) =>
            Task.FromResult(tenantId);
    }

    private sealed class StubTenantIdResolverFactory : ITenantIdResolverFactory
    {
        public Task<Guid?> GetTenantIdAsync(CancellationToken cancellationToken = default) => Task.FromResult<Guid?>(null);

        public Task<Guid?> GetTenantIdForOrgScopeAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult<Guid?>(null);
    }
}
