using System.ComponentModel.DataAnnotations;
using Platform.DDD.Domain.Shared.ExceptionHandling;

namespace Platform.Modules.IdentityAdmin;

/// <summary>Mã lỗi quản trị user nền tảng.</summary>
public class IdentityAdminErrorCode : IErrorCode
{
    [Display(Description = "Không tìm thấy user nền tảng.")]
    public const string PlatformUserNotFound = "98601";

    [Display(Description = "Không xóa được user nền tảng.")]
    public const string PlatformUserDeleteFailed = "98602";
}
