using System.Security.Claims;
using Platform.DDD.Domain.Services;
using Platform.DDD.Domain.Shared.Extensions;
using Microsoft.AspNetCore.Http;

namespace Platform.Authentication;

/// <summary>
/// Cổng đọc user hiện tại cho domain/application: một phương thức <see cref="GetAsync"/>.
/// </summary>
/// <remarks>
/// <para>
/// Thứ tự resolve (mỗi bước chỉ chạy khi bước trước không có kết quả):
/// </para>
/// <list type="number">
/// <item><description><see cref="ICurrentUserAccessor{TUser}.Current"/> — user ambient (job, test, middleware tùy chỉnh).</description></item>
/// <item><description>Claim <c>NameIdentifier</c> hoặc <c>sub</c> trên <see cref="HttpContext.User"/> sau <c>UseAuthentication</c>.</description></item>
/// <item><description><see cref="ICurrentUserStore{TUser}.FindAsync"/> — nạp profile đầy đủ (tên, tenant, …) theo id; không có trong store → <c>null</c>.</description></item>
/// </list>
/// <para>
/// Đăng ký scoped (<see cref="HostApplicationBuilderExtension.AddCurrentUser{TUser}"/>): cache <c>_resolved</c> / <c>_loaded</c>
/// chỉ trong một HTTP request (hoặc một scope DI), tránh gọi store lặp lại.
/// </para>
/// </remarks>
public sealed class CurrentUser<TUser>(
    ICurrentUserAccessor<TUser> currentUserAccessor,
    IHttpContextAccessor httpContextAccessor,
    ICurrentUserStore<TUser> currentUserStore) : ICurrentUser<TUser>
    where TUser : class, ICurrentUserIdentity
{
    // Kết quả sau lần tra store đầu tiên trong scope hiện tại (kể cả null).
    private TUser? _resolved;
    private bool _loaded;

    public async Task<TUser?> GetAsync(CancellationToken cancellationToken = default)
    {
        // Luồng non-HTTP hoặc code đã gọi BeginScope — không đọc token/store.
        var ambient = currentUserAccessor.Current;
        if (ambient != null)
            return ambient;

        // Đã resolve trong scope này (có hoặc không có user).
        if (_loaded)
            return _resolved;

        var userId = ParseGuidClaim(ClaimTypes.NameIdentifier, "sub");
        if (!userId.HasValue)
        {
            _loaded = true;
            _resolved = null;
            return null;
        }

        _resolved = await currentUserStore
            .FindAsync(userId.Value, cancellationToken)
            .ConfigureAwait(false);
        _loaded = true;
        return _resolved;
    }

    /// <summary>Lấy Guid user từ claim đầu tiên parse được (JWT thường dùng <c>sub</c>).</summary>
    private Guid? ParseGuidClaim(params string[] claimNames)
    {
        var user = httpContextAccessor.HttpContext?.User;
        if (user == null)
            return null;

        foreach (var name in claimNames)
        {
            var value = ClaimsPrincipalExtension.GetClaim(user.Claims, name)?.Value;
            if (Guid.TryParse(value, out var id))
                return id;
        }

        return null;
    }
}
