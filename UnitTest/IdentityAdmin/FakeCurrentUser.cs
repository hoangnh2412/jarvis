namespace UnitTest.IdentityAdmin;

using Platform.Authentication;
using Platform.DDD.Domain.Services;

/// <summary>
/// Fake đơn giản cho ICurrentUser — tránh phụ thuộc HttpContext trong unit test.
/// Cùng pattern với erp_core/platform/UnitTest/SignalR/FakeCurrentUser.cs.
/// </summary>
internal sealed class FakeCurrentUser(Guid? userId) : ICurrentUser<CurrentUserInfo>
{
    public Task<CurrentUserInfo?> GetAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(
            userId is { } id && id != Guid.Empty
                ? new CurrentUserInfo { UserId = id, UserName = "test-actor" }
                : null);
}
