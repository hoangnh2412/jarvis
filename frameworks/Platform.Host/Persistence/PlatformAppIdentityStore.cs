using Platform.Authorization.EntityFramework.Persistence;

namespace Platform.Host.Persistence;

/// <summary>Adapter mặc định cho <see cref="IAppIdentityStore"/> trên <see cref="IdentityDbContextBase"/> của product.</summary>
public sealed class PlatformAppIdentityStore(IdentityDbContextBase db) : IAppIdentityStore
{
    public IdentityDbContextBase DbContext => db;
}
