using Platform.DDD.Domain.Services;

namespace Platform.Authentication;

/// <summary>
/// Triển khai singleton <see cref="ICurrentUserAccessor{TUser}"/> lưu user “ambient” theo luồng async
/// (background job, message handler, test…) khi không có <c>HttpContext</c>.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="AsyncLocal{T}"/> gắn giá trị với <em>logical execution context</em> của .NET: cùng một instance
/// singleton dùng chung toàn app, nhưng mỗi request HTTP hoặc mỗi nhánh <c>Task</c> song song vẫn có
/// <see cref="Current"/> riêng — không đè lên nhau như biến static thường.
/// </para>
/// <para>
/// <see cref="ICurrentUser{TUser}.GetAsync"/> ưu tiên đọc <see cref="Current"/> trước; chỉ khi null mới
/// lấy claim từ token và <see cref="ICurrentUserStore{TUser}"/>. Gọi <see cref="BeginScope"/> để “giả lập”
/// user đã đăng nhập trong phạm vi <c>using</c>; khi <c>Dispose</c> scope, giá trị trước đó được khôi phục
/// (hỗ trợ scope lồng nhau).
/// </para>
/// </remarks>
public sealed class CurrentUserAccessor<TUser> : ICurrentUserAccessor<TUser>
    where TUser : class, ICurrentUserIdentity
{
    // Một field static cho cả process; giá trị thực tế tách theo async flow (xem remarks trên).
    private static readonly AsyncLocal<TUser?> CurrentUser = new();

    /// <summary>User ambient của luồng hiện tại; <c>null</c> nếu chưa gọi <see cref="BeginScope"/>.</summary>
    public TUser? Current => CurrentUser.Value;

    /// <summary>
    /// Đặt <paramref name="user"/> làm user ambient đến khi dispose đối tượng trả về;
    /// dispose sẽ trả <see cref="Current"/> về giá trị ngay trước lần gọi (stack scope).
    /// </summary>
    public IDisposable BeginScope(TUser? user)
    {
        var previous = CurrentUser.Value;
        CurrentUser.Value = user;
        return new RestoreScope(previous);
    }

    /// <summary>Khôi phục <see cref="AsyncLocal{T}"/> khi kết thúc <c>using</c> của <see cref="BeginScope"/>.</summary>
    private sealed class RestoreScope(TUser? previous) : IDisposable
    {
        public void Dispose() => CurrentUser.Value = previous;
    }
}
