using Jarvis.DDD.Domain.Services;

namespace UnitTest.SignalR;

internal sealed class TestTenantInfo : ICurrentTenantIdentity
{
    public Guid? TenantId { get; init; }
}
