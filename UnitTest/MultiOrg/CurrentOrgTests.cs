using Platform.DDD.Domain.DataStorages;
using Platform.DDD.Domain.Services;
using Platform.MultiOrg;
using Platform.MultiOrg.DataStorages;
using Platform.Multitenancy;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace UnitTest.MultiOrg;

/// <summary>Unit tests cho <c>Platform.MultiOrg</c> — parity <see cref="Multitenancy.CurrentTenantTests"/>.</summary>
public sealed class CurrentOrgTests
{
    [Fact]
    public void MO_01_Accessor_BeginScope_Sets_And_Restores()
    {
        var accessor = new CurrentOrgAccessor();
        var orgId = Guid.Parse("11111111-1111-1111-1111-111111111111");

        Assert.Null(accessor.OrgId);

        using (accessor.BeginScope(orgId))
            Assert.Equal(orgId, accessor.OrgId);

        Assert.Null(accessor.OrgId);
    }

    [Fact]
    public async Task MO_02_GetIdAsync_Prefers_Ambient_Over_Resolver()
    {
        var ambient = Guid.Parse("44444444-4444-4444-4444-444444444444");
        var resolved = Guid.Parse("55555555-5555-5555-5555-555555555555");
        var factory = new CountingOrgIdResolverFactory(resolved);
        var (org, accessor) = CreateOrg(factory, new EmptyOrgStore());

        using (accessor.BeginScope(ambient))
            Assert.Equal(ambient, await org.GetIdAsync());

        Assert.Equal(0, factory.CallCount);
    }

    [Fact]
    public async Task MO_03_GetIdAsync_Falls_Back_To_Resolver_When_Ambient_Empty()
    {
        var resolved = Guid.Parse("66666666-6666-6666-6666-666666666666");
        var (org, _) = CreateOrg(new CountingOrgIdResolverFactory(resolved), new EmptyOrgStore());

        Assert.Equal(resolved, await org.GetIdAsync());
    }

    [Fact]
    public void MO_04_AddCurrentOrg_Registers_Accessor_And_CurrentOrg()
    {
        var builder = Host.CreateApplicationBuilder();
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddSingleton<ICurrentTenantAccessor, CurrentTenantAccessor>();
        builder.Services.AddSingleton<ITenantIdResolverFactory>(new StubTenantIdResolverFactory());
        builder.Services.AddSingleton<IOrgTenantScopeValidator, NoOpOrgTenantValidator>();
        builder.Services.AddSingleton<ICurrentOrgStore<CurrentOrgInfo>, EmptyOrgStore>();
        builder.AddCurrentOrg<CurrentOrgInfo>();
        builder.Services.AddSingleton<IOrgIdResolverFactory>(new CountingOrgIdResolverFactory(null));

        using var host = builder.Build();
        using var scope = host.Services.CreateScope();

        var accessor1 = host.Services.GetRequiredService<ICurrentOrgAccessor>();
        var accessor2 = host.Services.GetRequiredService<ICurrentOrgAccessor>();
        Assert.Same(accessor1, accessor2);
        Assert.IsType<CurrentOrgAccessor>(accessor1);

        var org = scope.ServiceProvider.GetRequiredService<ICurrentOrg<CurrentOrgInfo>>();
        Assert.IsType<CurrentOrg<CurrentOrgInfo>>(org);
    }

    [Fact]
    public void MO_05_AddCurrentOrg_Registers_Default_OrgIdResolverFactory()
    {
        var builder = Host.CreateApplicationBuilder();
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddSingleton<ICurrentTenantAccessor, CurrentTenantAccessor>();
        builder.Services.AddSingleton<ITenantIdResolverFactory>(new StubTenantIdResolverFactory());
        builder.Services.AddSingleton<IOrgTenantScopeValidator, NoOpOrgTenantValidator>();
        builder.Services.AddSingleton<ICurrentOrgStore<CurrentOrgInfo>, EmptyOrgStore>();
        builder.AddCurrentOrg<CurrentOrgInfo>();

        using var host = builder.Build();
        using var scope = host.Services.CreateScope();

        Assert.IsType<OrgIdResolverFactory>(
            scope.ServiceProvider.GetRequiredService<IOrgIdResolverFactory>());
        Assert.IsType<HeaderOrgIdResolver>(
            scope.ServiceProvider.GetRequiredKeyedService<IOrgIdResolver>(nameof(HeaderOrgIdResolver)));
    }

    [Fact]
    public async Task MO_07_HeaderOrgIdResolver_Reads_X_Org_Id()
    {
        var orgId = Guid.Parse("77777777-7777-7777-7777-777777777777");
        var accessor = new HttpContextAccessor
        {
            HttpContext = new DefaultHttpContext(),
        };
        accessor.HttpContext!.Request.Headers[HeaderOrgIdResolver.HeaderName] = orgId.ToString();

        var resolver = new HeaderOrgIdResolver(accessor);
        Assert.Equal(orgId, await resolver.GetOrgIdAsync());
    }

    [Fact]
    public async Task MO_06_GetAsync_Uses_Store_After_Resolved_Id()
    {
        var orgId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
        var store = new FixedOrgStore(new CurrentOrgInfo { OrgId = orgId, Code = "acme", Name = "Acme" });
        var (org, _) = CreateOrg(new CountingOrgIdResolverFactory(orgId), store);

        var info = await org.GetAsync();
        Assert.NotNull(info);
        Assert.Equal(orgId, info!.OrgId);
        Assert.Equal("acme", info.Code);
        Assert.Equal(info, await org.GetAsync());
        Assert.Equal(1, store.CallCount);
    }

    private static (ICurrentOrg<CurrentOrgInfo> Org, ICurrentOrgAccessor Accessor) CreateOrg(
        IOrgIdResolverFactory factory,
        ICurrentOrgStore<CurrentOrgInfo> store)
    {
        var accessor = new CurrentOrgAccessor();
        ICurrentOrg<CurrentOrgInfo> org = new CurrentOrg<CurrentOrgInfo>(accessor, factory, store);
        return (org, accessor);
    }

    private sealed class CountingOrgIdResolverFactory(Guid? orgId) : IOrgIdResolverFactory
    {
        public int CallCount { get; private set; }

        public Task<Guid?> GetOrgIdAsync(CancellationToken cancellationToken = default)
        {
            CallCount++;
            return Task.FromResult(orgId);
        }
    }

    private sealed class EmptyOrgStore : ICurrentOrgStore<CurrentOrgInfo>
    {
        public Task<CurrentOrgInfo?> FindAsync(Guid orgId, CancellationToken cancellationToken = default)
            => Task.FromResult<CurrentOrgInfo?>(null);
    }

    private sealed class StubTenantIdResolverFactory : ITenantIdResolverFactory
    {
        public Task<Guid?> GetTenantIdAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult<Guid?>(null);

        public Task<Guid?> GetTenantIdForOrgScopeAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult<Guid?>(null);
    }

    private sealed class NoOpOrgTenantValidator : IOrgTenantScopeValidator
    {
        public Task ValidateOrgBelongsToTenantAsync(Guid orgId, Guid tenantId, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;
    }

    private sealed class FixedOrgStore(CurrentOrgInfo info) : ICurrentOrgStore<CurrentOrgInfo>
    {
        public int CallCount { get; private set; }

        public Task<CurrentOrgInfo?> FindAsync(Guid orgId, CancellationToken cancellationToken = default)
        {
            CallCount++;
            return Task.FromResult<CurrentOrgInfo?>(info.OrgId == orgId ? info : null);
        }
    }
}
