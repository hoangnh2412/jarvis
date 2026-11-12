namespace Platform.Authorization.EntityFramework.Persistence;

/// <summary>Cửa truy cập identity store tầng App (database nghiệp vụ của tenant). Xem <see cref="IPlatformIdentityStore"/>.</summary>
public interface IAppIdentityStore
{
    IdentityDbContextBase DbContext { get; }
}
