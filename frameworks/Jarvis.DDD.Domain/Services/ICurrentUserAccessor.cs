namespace Jarvis.DDD.Domain.Services;

/// <summary>
/// Ambient user cho luồng non-HTTP. Code ứng dụng nên dùng <see cref="ICurrentUser{TUser}"/>.
/// </summary>
public interface ICurrentUserAccessor<TUser> where TUser : class, ICurrentUserIdentity
{
    TUser? Current { get; }

    IDisposable BeginScope(TUser? user);
}
