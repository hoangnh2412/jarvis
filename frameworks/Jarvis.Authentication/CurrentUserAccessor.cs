using Jarvis.DDD.Domain.Services;

namespace Jarvis.Authentication;

/// <summary>
/// Singleton <see cref="ICurrentUserAccessor{TUser}"/> dùng <see cref="AsyncLocal{T}"/>.
/// </summary>
public sealed class CurrentUserAccessor<TUser> : ICurrentUserAccessor<TUser>
    where TUser : class, ICurrentUserIdentity
{
    private static readonly AsyncLocal<TUser?> CurrentUser = new();

    public TUser? Current => CurrentUser.Value;

    public IDisposable BeginScope(TUser? user)
    {
        var previous = CurrentUser.Value;
        CurrentUser.Value = user;
        return new RestoreScope(previous);
    }

    private sealed class RestoreScope(TUser? previous) : IDisposable
    {
        public void Dispose() => CurrentUser.Value = previous;
    }
}
