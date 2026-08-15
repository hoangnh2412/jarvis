using Jarvis.Authentication;
using Jarvis.DDD.Domain.Services;

namespace UnitTest.SignalR;

internal sealed class FakeCurrentUser(Guid? userId) : ICurrentUser<CurrentUserInfo>
{
    public Task<CurrentUserInfo?> GetAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(
            userId is { } id && id != Guid.Empty
                ? new CurrentUserInfo { UserId = id, UserName = "test-user" }
                : null);
}
