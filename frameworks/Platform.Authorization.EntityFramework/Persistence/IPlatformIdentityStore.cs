namespace Platform.Authorization.EntityFramework.Persistence;

/// <summary>
/// Cửa truy cập identity store tầng Platform (Master). Host đăng ký adapter trên unit of work của mình;
/// module identity dùng interface này thay vì DbContext cụ thể của host.
/// </summary>
public interface IPlatformIdentityStore
{
    IdentityDbContextBase DbContext { get; }
}
