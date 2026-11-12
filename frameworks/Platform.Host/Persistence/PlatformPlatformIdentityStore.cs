using Platform.Authorization.EntityFramework.Persistence;

namespace Platform.Host.Persistence;

/// <summary>Adapter mặc định cho <see cref="IPlatformIdentityStore"/> trên <see cref="IdentityDbContextBase"/> của Master.</summary>
public sealed class PlatformPlatformIdentityStore(IdentityDbContextBase db) : IPlatformIdentityStore
{
    public IdentityDbContextBase DbContext => db;
}
