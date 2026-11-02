using Platform.Authentication;
using Platform.Authorization.Abstractions;
using Platform.DDD.Domain.Services;

namespace Platform.Modules.IdentityAdmin.Authorization;

/// <summary>Kiểm tra quyền phía API thay vì dựa vào guard chỉ chạy trong trình duyệt.</summary>
public sealed class PermissionAuthorization(
    ICurrentUser<CurrentUserInfo> currentUser,
    IPermissionProvider permissions)
{
    /// <summary>Trả về quyền của người dùng hiện hành, bao gồm wildcard do provider xử lý.</summary>
    public async Task<bool> HasAsync(string permission, CancellationToken cancellationToken)
    {
        var actor = await currentUser.GetAsync(cancellationToken).ConfigureAwait(false);
        return actor?.UserId is Guid userId
            && await permissions.HasPermissionAsync(userId, permission, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>Wildcard / System.All cũng được tính, không phụ thuộc provider có mở rộng catalog hay không.</summary>
    public async Task<bool> GrantsAsync(string permission, CancellationToken cancellationToken)
    {
        var actor = await currentUser.GetAsync(cancellationToken).ConfigureAwait(false);
        if (actor?.UserId is not Guid userId)
            return false;

        var granted = await permissions.GetPermissionsAsync(userId, cancellationToken).ConfigureAwait(false);
        return PermissionCatalog.Grants(granted, permission);
    }
}
