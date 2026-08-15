using System.Security.Claims;
using Jarvis.Authentication;
using Jarvis.DDD.Domain.DataStorages;
using Jarvis.DDD.Domain.Services;
using Jarvis.Multitenancy;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace UnitTest.Services;

public sealed class CurrentUserTenantTests
{
    [Fact]
    public async Task CurrentUser_GetAsync_Uses_Token_UserId_Then_Store()
    {
        var userId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var homeTenantId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
        var http = CreateHttp(userId);

        var store = new FixedCurrentUserStore(new CurrentUserInfo
        {
            UserId = userId,
            UserName = "alice-from-store",
            TenantId = homeTenantId,
        });
        var user = CreateCurrentUser(http, store);

        var info = await user.GetAsync();
        Assert.NotNull(info);
        Assert.Equal(userId, info!.UserId);
        Assert.Equal("alice-from-store", info.UserName);
        Assert.Equal(homeTenantId, info.TenantId);
        Assert.Null(info.TokenId); // payload từ store không có token
        Assert.Equal(info, await user.GetAsync());
    }

    [Fact]
    public async Task CurrentUser_GetAsync_Store_Returns_Null_Yields_Null()
    {
        var userId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var http = CreateHttp(userId);

        var user = CreateCurrentUser(http, new EmptyCurrentUserStore());

        Assert.Null(await user.GetAsync());
    }

    [Fact]
    public async Task CurrentUser_GetAsync_Prefers_Ambient()
    {
        var ambientId = Guid.Parse("33333333-3333-3333-3333-333333333333");
        var httpId = Guid.Parse("44444444-4444-4444-4444-444444444444");
        var accessor = new CurrentUserAccessor<CurrentUserInfo>();
        var http = CreateHttp(httpId);

        var user = new CurrentUser<CurrentUserInfo>(
            accessor,
            http,
            new EmptyCurrentUserStore());

        using (accessor.BeginScope(new CurrentUserInfo { UserId = ambientId, UserName = "job-user" }))
        {
            var info = await user.GetAsync();
            Assert.Equal(ambientId, info?.UserId);
            Assert.Equal("job-user", info?.UserName);
        }

        Assert.Null(await user.GetAsync()); // HTTP có userId nhưng store trống
    }

    [Fact]
    public async Task CurrentUser_GetAsync_Without_HttpContext_Uses_Ambient_Only()
    {
        var accessor = new CurrentUserAccessor<CurrentUserInfo>();
        var user = new CurrentUser<CurrentUserInfo>(
            accessor,
            new HttpContextAccessor(),
            new EmptyCurrentUserStore());

        Assert.Null(await user.GetAsync());

        var userId = Guid.Parse("66666666-6666-6666-6666-666666666666");
        using (accessor.BeginScope(new CurrentUserInfo { UserId = userId, UserName = "worker" }))
        {
            var info = await user.GetAsync();
            Assert.Equal(userId, info?.UserId);
            Assert.Equal("worker", info?.UserName);
        }
    }

    [Fact]
    public async Task D5_Home_Tenant_Stays_On_User_When_Current_Tenant_Changes()
    {
        var userId = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc");
        var homeTenantId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
        var workingTenantId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");

        var services = new ServiceCollection();
        services.AddSingleton<IHttpContextAccessor>(CreateHttp(userId));
        services.AddSingleton<ICurrentTenantAccessor, CurrentTenantAccessor>();
        services.AddSingleton<ICurrentUserAccessor<CurrentUserInfo>, CurrentUserAccessor<CurrentUserInfo>>();
        services.AddSingleton<ICurrentUserStore<CurrentUserInfo>>(new FixedCurrentUserStore(new CurrentUserInfo
        {
            UserId = userId,
            TenantId = homeTenantId,
        }));
        services.AddSingleton<ICurrentTenantStore<CurrentTenantInfo>, EmptyCurrentTenantStore>();
        services.AddSingleton<ITenantIdResolverFactory>(new FixedTenantIdResolverFactory(homeTenantId));
        services.AddScoped<ICurrentUser<CurrentUserInfo>, CurrentUser<CurrentUserInfo>>();
        services.AddScoped<ICurrentTenant<CurrentTenantInfo>, CurrentTenant<CurrentTenantInfo>>();
        var sp = services.BuildServiceProvider();

        using var scope = sp.CreateScope();
        var user = scope.ServiceProvider.GetRequiredService<ICurrentUser<CurrentUserInfo>>();
        var tenant = scope.ServiceProvider.GetRequiredService<ICurrentTenant<CurrentTenantInfo>>();

        Assert.Equal(homeTenantId, (await user.GetAsync())?.TenantId);
        Assert.Equal(homeTenantId, await tenant.GetIdAsync());

        using (tenant.Change(workingTenantId))
        {
            Assert.Equal(homeTenantId, (await user.GetAsync())?.TenantId);
            Assert.Equal(workingTenantId, await tenant.GetIdAsync());
        }

        Assert.Equal(homeTenantId, (await user.GetAsync())?.TenantId);
        Assert.Equal(homeTenantId, await tenant.GetIdAsync());
    }

    [Fact]
    public async Task Custom_TUser_Exposes_FirstName_LastName()
    {
        var userId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
        var http = CreateHttp(userId);
        var store = new FixedAppUserStore(new AppUser
        {
            UserId = userId,
            FirstName = "Ada",
            LastName = "Lovelace",
            UserName = "ada",
        });

        var user = new CurrentUser<AppUser>(
            new CurrentUserAccessor<AppUser>(),
            http,
            store);

        var info = await user.GetAsync();
        Assert.Equal("Ada", info?.FirstName);
        Assert.Equal("Lovelace", info?.LastName);
    }

    private static CurrentUser<CurrentUserInfo> CreateCurrentUser(
        IHttpContextAccessor http,
        ICurrentUserStore<CurrentUserInfo> store) =>
        new(new CurrentUserAccessor<CurrentUserInfo>(), http, store);

    private static HttpContextAccessor CreateHttp(Guid userId, Guid? homeTenantId = null)
    {
        var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, userId.ToString()) };
        if (homeTenantId.HasValue)
            claims.Add(new Claim(ClaimTypes.GroupSid, homeTenantId.Value.ToString()));

        return new HttpContextAccessor
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(claims, authenticationType: "Test")),
            },
        };
    }

    private sealed class FixedTenantIdResolverFactory(Guid? tenantId) : ITenantIdResolverFactory
    {
        public Task<Guid?> GetTenantIdAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(tenantId);
    }

    private sealed class FixedCurrentUserStore(CurrentUserInfo info) : ICurrentUserStore<CurrentUserInfo>
    {
        public Task<CurrentUserInfo?> FindAsync(Guid userId, CancellationToken cancellationToken = default)
            => Task.FromResult<CurrentUserInfo?>(info);
    }

    private sealed class EmptyCurrentUserStore : ICurrentUserStore<CurrentUserInfo>
    {
        public Task<CurrentUserInfo?> FindAsync(Guid userId, CancellationToken cancellationToken = default)
            => Task.FromResult<CurrentUserInfo?>(null);
    }

    private sealed class EmptyCurrentTenantStore : ICurrentTenantStore<CurrentTenantInfo>
    {
        public Task<CurrentTenantInfo?> FindAsync(Guid tenantId, CancellationToken cancellationToken = default)
            => Task.FromResult<CurrentTenantInfo?>(null);
    }

    private sealed class AppUser : ICurrentUserIdentity
    {
        public Guid? UserId { get; init; }
        public Guid? TokenId { get; init; }
        public Guid? TenantId { get; init; }
        public string? UserName { get; init; }
        public string? FirstName { get; init; }
        public string? LastName { get; init; }
    }

    private sealed class FixedAppUserStore(AppUser info) : ICurrentUserStore<AppUser>
    {
        public Task<AppUser?> FindAsync(Guid userId, CancellationToken cancellationToken = default)
            => Task.FromResult<AppUser?>(info);
    }
}
