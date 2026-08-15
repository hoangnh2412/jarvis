namespace Jarvis.DDD.Domain.Services;

/// <summary>
/// User hiện tại. Một cổng đọc: <see cref="GetAsync"/>.
/// </summary>
public interface ICurrentUser<TUser> where TUser : class, ICurrentUserIdentity
{
    /// <summary>
    /// Ambient → user id từ token → <see cref="ICurrentUserStore{TUser}"/> (cache trên scoped instance).
    /// Không có profile trong store thì trả <c>null</c> — không fallback.
    /// </summary>
    Task<TUser?> GetAsync(CancellationToken cancellationToken = default);
}
