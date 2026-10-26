using System.ComponentModel.DataAnnotations;
using Platform.DDD.Domain.Shared.ExceptionHandling;

namespace Platform.Tenants;

/// <summary>Mã lỗi catalog tenant.</summary>
public class TenantErrorCode : IErrorCode
{
    [Display(Description = "Không tìm thấy tenant")]
    public const string NotFound = "98401";

    [Display(Description = "Mã tenant đã tồn tại")]
    public const string CodeAlreadyExists = "98402";

    [Display(Description = "Dữ liệu tenant không hợp lệ")]
    public const string InvalidInput = "98403";

    [Display(Description = "Tenant cha không hợp lệ hoặc tạo vòng")]
    public const string InvalidParent = "98404";

    [Display(Description = "Tenant cha đã vô hiệu hóa")]
    public const string ParentInactive = "98405";

    [Display(Description = "Tenant đã vô hiệu hóa")]
    public const string Inactive = "98406";
}
