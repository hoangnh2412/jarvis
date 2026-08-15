namespace Jarvis.DDD.Domain.Services;

/// <summary>
/// Load full profile user (DB/cache). Host bắt buộc đăng ký implementation thật.
/// </summary>
public interface ICurrentUserStore<TUser> where TUser : class, ICurrentUserIdentity
{
    Task<TUser?> FindAsync(Guid userId, CancellationToken cancellationToken = default);
}
