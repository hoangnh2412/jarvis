using Jarvis.DDD.Domain.DataStorages;
using Jarvis.DDD.Domain.Services;
using Jarvis.Multitenancy;
using Jarvis.Multitenancy.DataStorages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace UnitTest.Multitenancy;

/// <summary>Unit tests cho <c>Jarvis.Multitenancy</c> — accessor, R2, <see cref="ICurrentTenant{TTenant}"/>, store.</summary>
public sealed class CurrentTenantTests
{
    [Fact]
    public void MT_01_Accessor_BeginScope_Sets_And_Restores()
    {
        var accessor = new CurrentTenantAccessor();
        var tenantId = Guid.Parse("11111111-1111-1111-1111-111111111111");

        Assert.Null(accessor.TenantId);

        using (accessor.BeginScope(tenantId))
            Assert.Equal(tenantId, accessor.TenantId);

        Assert.Null(accessor.TenantId);
    }

    [Fact]
    public void MT_02_Accessor_Nested_BeginScope_Restores_Outer()
    {
        var accessor = new CurrentTenantAccessor();
        var outer = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var inner = Guid.Parse("33333333-3333-3333-3333-333333333333");

        using (accessor.BeginScope(outer))
        {
            Assert.Equal(outer, accessor.TenantId);
            using (accessor.BeginScope(inner))
                Assert.Equal(inner, accessor.TenantId);
            Assert.Equal(outer, accessor.TenantId);
        }

        Assert.Null(accessor.TenantId);
    }

    [Fact]
    public async Task MT_03_GetIdAsync_Prefers_Ambient_Over_Resolver()
    {
        var ambient = Guid.Parse("44444444-4444-4444-4444-444444444444");
        var resolved = Guid.Parse("55555555-5555-5555-5555-555555555555");
        var factory = new CountingTenantIdResolverFactory(resolved);
        var (tenant, accessor) = CreateTenant(factory, new EmptyTenantStore());

        using (accessor.BeginScope(ambient))
            Assert.Equal(ambient, await tenant.GetIdAsync());

        Assert.Equal(0, factory.CallCount);
    }

    [Fact]
    public async Task MT_04_GetIdAsync_Falls_Back_To_Resolver_When_Ambient_Empty()
    {
        var resolved = Guid.Parse("66666666-6666-6666-6666-666666666666");
        var (tenant, _) = CreateTenant(new CountingTenantIdResolverFactory(resolved), new EmptyTenantStore());

        Assert.Equal(resolved, await tenant.GetIdAsync());
    }

    [Fact]
    public async Task MT_05_GetIdAsync_Caches_Resolver_Result_On_Scoped_Instance()
    {
        var resolved = Guid.Parse("77777777-7777-7777-7777-777777777777");
        var factory = new CountingTenantIdResolverFactory(resolved);
        var (tenant, _) = CreateTenant(factory, new EmptyTenantStore());

        Assert.Equal(resolved, await tenant.GetIdAsync());
        Assert.Equal(resolved, await tenant.GetIdAsync());
        Assert.Equal(1, factory.CallCount);
    }

    [Fact]
    public async Task MT_06_Change_Sets_And_Restores_Ambient()
    {
        var (tenant, accessor) = CreateTenant(new CountingTenantIdResolverFactory(null), new EmptyTenantStore());
        var tenantId = Guid.Parse("88888888-8888-8888-8888-888888888888");

        Assert.Null(await tenant.GetIdAsync());

        using (tenant.Change(tenantId))
        {
            Assert.Equal(tenantId, accessor.TenantId);
            Assert.Equal(tenantId, await tenant.GetIdAsync());
        }

        Assert.Null(accessor.TenantId);
    }

    [Fact]
    public void MT_07_Change_Empty_Guid_Throws()
    {
        var (tenant, _) = CreateTenant(new CountingTenantIdResolverFactory(null), new EmptyTenantStore());

        var ex = Assert.Throws<ArgumentException>(() => tenant.Change(Guid.Empty));
        Assert.Equal("tenantId", ex.ParamName);
    }

    [Fact]
    public void MT_08_AddCurrentTenant_Registers_Accessor_And_CurrentTenant()
    {
        var builder = Host.CreateApplicationBuilder();
        builder.Services.AddSingleton<ITenantIdResolverFactory>(new CountingTenantIdResolverFactory(null));
        builder.Services.AddSingleton<ICurrentTenantStore<CurrentTenantInfo>, EmptyTenantStore>();
        builder.AddCurrentTenant<CurrentTenantInfo>();

        using var host = builder.Build();
        using var scope = host.Services.CreateScope();

        var accessor1 = host.Services.GetRequiredService<ICurrentTenantAccessor>();
        var accessor2 = host.Services.GetRequiredService<ICurrentTenantAccessor>();
        Assert.Same(accessor1, accessor2);
        Assert.IsType<CurrentTenantAccessor>(accessor1);

        var tenant = scope.ServiceProvider.GetRequiredService<ICurrentTenant<CurrentTenantInfo>>();
        Assert.IsType<CurrentTenant<CurrentTenantInfo>>(tenant);
    }

    [Fact]
    public void MT_08b_AddCurrentTenant_Registers_Default_TenantIdResolverFactory()
    {
        var builder = Host.CreateApplicationBuilder();
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddSingleton<ICurrentTenantStore<CurrentTenantInfo>, EmptyTenantStore>();
        builder.AddCurrentTenant<CurrentTenantInfo>();

        using var host = builder.Build();
        using var scope = host.Services.CreateScope();

        Assert.IsType<TenantIdResolverFactory>(
            scope.ServiceProvider.GetRequiredService<ITenantIdResolverFactory>());
        Assert.IsType<HeaderTenantIdResolver>(
            scope.ServiceProvider.GetRequiredKeyedService<ITenantIdResolver>(nameof(HeaderTenantIdResolver)));
    }

    [Fact]
    public void MT_09_AddCurrentTenant_Is_Idempotent()
    {
        var builder = Host.CreateApplicationBuilder();
        builder.Services.AddSingleton<ITenantIdResolverFactory>(new CountingTenantIdResolverFactory(null));
        builder.Services.AddSingleton<ICurrentTenantStore<CurrentTenantInfo>, EmptyTenantStore>();
        builder.AddCurrentTenant<CurrentTenantInfo>();
        builder.AddCurrentTenant<CurrentTenantInfo>();

        using var host = builder.Build();
        Assert.Single(host.Services.GetServices<ICurrentTenantAccessor>());
    }

    [Fact]
    public async Task MT_10_GetAsync_Uses_Store_After_Resolved_Id()
    {
        var tenantId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
        var store = new FixedTenantStore(new CurrentTenantInfo { TenantId = tenantId, Name = "acme" });
        var (tenant, _) = CreateTenant(new CountingTenantIdResolverFactory(tenantId), store);

        var info = await tenant.GetAsync();
        Assert.NotNull(info);
        Assert.Equal(tenantId, info!.TenantId);
        Assert.Equal("acme", info.Name);
        Assert.Equal(info, await tenant.GetAsync());
        Assert.Equal(1, store.CallCount);
    }

    [Fact]
    public async Task MT_11_GetAsync_Reloads_When_Change_Switches_Tenant()
    {
        var a = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
        var b = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc");
        var store = new MapTenantStore(new Dictionary<Guid, CurrentTenantInfo>
        {
            [a] = new() { TenantId = a, Name = "A" },
            [b] = new() { TenantId = b, Name = "B" },
        });
        var (tenant, _) = CreateTenant(new CountingTenantIdResolverFactory(a), store);

        Assert.Equal("A", (await tenant.GetAsync())?.Name);

        using (tenant.Change(b))
            Assert.Equal("B", (await tenant.GetAsync())?.Name);

        Assert.Equal("A", (await tenant.GetAsync())?.Name);
    }

    private static (ICurrentTenant<CurrentTenantInfo> Tenant, ICurrentTenantAccessor Accessor) CreateTenant(
        ITenantIdResolverFactory factory,
        ICurrentTenantStore<CurrentTenantInfo> store)
    {
        var accessor = new CurrentTenantAccessor();
        ICurrentTenant<CurrentTenantInfo> tenant = new CurrentTenant<CurrentTenantInfo>(accessor, factory, store);
        return (tenant, accessor);
    }

    private sealed class CountingTenantIdResolverFactory(Guid? tenantId) : ITenantIdResolverFactory
    {
        public int CallCount { get; private set; }

        public Task<Guid?> GetTenantIdAsync(CancellationToken cancellationToken = default)
        {
            CallCount++;
            return Task.FromResult(tenantId);
        }
    }

    private sealed class EmptyTenantStore : ICurrentTenantStore<CurrentTenantInfo>
    {
        public Task<CurrentTenantInfo?> FindAsync(Guid tenantId, CancellationToken cancellationToken = default)
            => Task.FromResult<CurrentTenantInfo?>(null);
    }

    private sealed class FixedTenantStore(CurrentTenantInfo info) : ICurrentTenantStore<CurrentTenantInfo>
    {
        public int CallCount { get; private set; }

        public Task<CurrentTenantInfo?> FindAsync(Guid tenantId, CancellationToken cancellationToken = default)
        {
            CallCount++;
            return Task.FromResult<CurrentTenantInfo?>(info.TenantId == tenantId ? info : null);
        }
    }

    private sealed class MapTenantStore(Dictionary<Guid, CurrentTenantInfo> map) : ICurrentTenantStore<CurrentTenantInfo>
    {
        public Task<CurrentTenantInfo?> FindAsync(Guid tenantId, CancellationToken cancellationToken = default)
            => Task.FromResult(map.TryGetValue(tenantId, out var info) ? info : null);
    }
}
