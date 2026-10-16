using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Platform.Realtime.Extensions;
using Platform.Realtime.Services;
using Platform.Realtime.SignalR.Extensions;
using StackExchange.Redis;

namespace UnitTest.Realtime;

public class RealtimeRedisMultiplexerTests
{
    [Fact]
    public void UseRedisBackplane_Registers_Single_Shared_Multiplexer()
    {
        var builder = WebApplication.CreateBuilder();
        builder.Configuration["Realtime:Redis:Configuration"] = "127.0.0.1:6379,abortConnect=false";

        builder.AddCoreRealtime(o => o.SignalR.UseRedisBackplane = true)
            .UseSignalR<Platform.Authentication.CurrentUserInfo, TestTenantInfo>();

        using var app = builder.Build();
        var multiplexer = app.Services.GetRequiredService<IRealtimeRedisMultiplexer>().Multiplexer;

        Assert.True(multiplexer.IsConnected);
    }

    private sealed class TestTenantInfo : Platform.DDD.Domain.Services.ICurrentTenantIdentity
    {
        public Guid? TenantId { get; init; }
    }
}
