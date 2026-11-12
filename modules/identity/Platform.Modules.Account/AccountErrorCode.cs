using System.ComponentModel.DataAnnotations;
using Platform.DDD.Domain.Shared.ExceptionHandling;

namespace Platform.Modules.Account;

/// <summary>Mã lỗi tài khoản. Message mặc định lấy từ <see cref="DisplayAttribute.Description"/>.</summary>
public class AccountErrorCode : IErrorCode
{
    [Display(Description = "Tên đăng nhập hoặc mật khẩu không đúng.")]
    public const string InvalidCredentials = "98001";

    [Display(Description = "Tài khoản đã bị khóa.")]
    public const string AccountLocked = "98002";

    [Display(Description = "Tenant không tồn tại hoặc đã ngừng hoạt động.")]
    public const string TenantUnavailable = "98003";

    [Display(Description = "Organization không tồn tại hoặc đã ngừng hoạt động.")]
    public const string OrgUnavailable = "98004";

    [Display(Description = "Refresh token không hợp lệ hoặc đã hết hạn.")]
    public const string RefreshTokenInvalid = "98005";

    [Display(Description = "Thiếu thông tin đăng nhập.")]
    public const string LoginInputRequired = "98006";

    [Display(Description = "Phiên đăng nhập không hợp lệ.")]
    public const string SessionInvalid = "98007";

    [Display(Description = "Không tìm thấy người dùng.")]
    public const string UserNotFound = "98008";

    [Display(Description = "Không đổi được mật khẩu.")]
    public const string ChangePasswordFailed = "98009";

    [Display(Description = "Không cập nhật được hồ sơ.")]
    public const string UpdateProfileFailed = "98010";

    [Display(Description = "Token hoặc email đặt lại mật khẩu không hợp lệ.")]
    public const string ResetPasswordInvalid = "98011";

    [Display(Description = "Không đặt lại được mật khẩu.")]
    public const string ResetPasswordFailed = "98012";

    [Display(Description = "Tài khoản không thuộc tầng platform.")]
    public const string NotPlatformAccount = "98013";
}
