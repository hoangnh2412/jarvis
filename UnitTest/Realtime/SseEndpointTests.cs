using System.Net;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Platform.Authentication;
using Platform.DDD.Domain.Services;
using Platform.Realtime.Extensions;
using Platform.Realtime.Sse.Extensions;

namespace UnitTest.Realtime;

public class SseEndpointTests
{
    private sealed class TestTenantInfo : ICurrentTenantIdentity
    {
        public Guid? TenantId { get; init; }
    }

    private sealed class StubCurrentUser(Guid userId) : ICurrentUser<CurrentUserInfo>
    {
        public Task<CurrentUserInfo?> GetAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult<CurrentUserInfo?>(new CurrentUserInfo
            {
                UserId = userId,
                TokenId = Guid.NewGuid(),
                TenantId = Guid.Parse("22222222-2222-2222-2222-222222222222")
            });
    }

    private sealed class StubCurrentTenant(Guid? tenantId) : ICurrentTenant<TestTenantInfo>
    {
        public Task<Guid?> GetIdAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(tenantId);

        public Task<TestTenantInfo?> GetAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(tenantId is { } id ? new TestTenantInfo { TenantId = id } : null);

        public IDisposable Change(Guid tenantId) => NullScope.Instance;

        private sealed class NullScope : IDisposable
        {
            public static readonly NullScope Instance = new();
            public void Dispose() { }
        }
    }

    private sealed class TestAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder) : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
    {
        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var identity = new ClaimsIdentity(
                [new Claim(ClaimTypes.NameIdentifier, "11111111-1111-1111-1111-111111111111")],
                "Test");
            var ticket = new AuthenticationTicket(new ClaimsPrincipal(identity), "Test");
            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }

    [Fact]
    public async Task MapRealtimeSse_Returns_EventStream_And_Heartbeat()
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Configuration["Realtime:Sse:SsePath"] = "/sse/notifications";
        builder.Configuration["Realtime:Sse:HeartbeatSeconds"] = "1";

        builder.Services
            .AddAuthentication("Test")
            .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", _ => { });
        builder.Services.AddAuthorization();
        builder.Services.AddSingleton<ICurrentUser<CurrentUserInfo>>(
            new StubCurrentUser(Guid.Parse("11111111-1111-1111-1111-111111111111")));
        builder.Services.AddSingleton<ICurrentTenant<TestTenantInfo>>(
            new StubCurrentTenant(Guid.Parse("22222222-2222-2222-2222-222222222222")));

        builder.AddCoreRealtime().UseSse<CurrentUserInfo, TestTenantInfo>();

        var app = builder.Build();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapRealtimeSse<CurrentUserInfo, TestTenantInfo>();
        await app.StartAsync();

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(3));
        var client = app.GetTestClient();
        using var response = await client.GetAsync("/sse/notifications", HttpCompletionOption.ResponseHeadersRead, cts.Token);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("text/event-stream", response.Content.Headers.ContentType?.MediaType);

        await using var stream = await response.Content.ReadAsStreamAsync(cts.Token);
        using var reader = new StreamReader(stream, Encoding.UTF8);
        var firstChunk = await reader.ReadLineAsync(cts.Token);
        Assert.StartsWith(": heartbeat", firstChunk);
    }

    [Fact]
    public async Task MapRealtimeSse_Missing_Tenant_Returns_401()
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Configuration["Realtime:Sse:SsePath"] = "/sse/notifications";

        builder.Services
            .AddAuthentication("Test")
            .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", _ => { });
        builder.Services.AddAuthorization();
        builder.Services.AddSingleton<ICurrentUser<CurrentUserInfo>>(
            new StubCurrentUser(Guid.Parse("11111111-1111-1111-1111-111111111111")));
        builder.Services.AddSingleton<ICurrentTenant<TestTenantInfo>>(new StubCurrentTenant(null));

        builder.AddCoreRealtime().UseSse<CurrentUserInfo, TestTenantInfo>();

        var app = builder.Build();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapRealtimeSse<CurrentUserInfo, TestTenantInfo>();
        await app.StartAsync();

        var client = app.GetTestClient();
        using var response = await client.GetAsync("/sse/notifications");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
