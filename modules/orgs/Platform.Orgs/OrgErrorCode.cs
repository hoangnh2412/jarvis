using System.ComponentModel.DataAnnotations;
using Platform.DDD.Domain.Shared.ExceptionHandling;

namespace Platform.Orgs;

/// <summary>Mã lỗi catalog Org.</summary>
public class OrgErrorCode : IErrorCode
{
    [Display(Description = "Hệ thống chưa xác định được tenant đang thao tác")]
    public const string TenantRequired = "98301";

    [Display(Description = "Không tìm thấy organization trong tenant")]
    public const string NotFound = "98302";

    [Display(Description = "Mã organization đã tồn tại trong tenant")]
    public const string CodeAlreadyExists = "98303";

    [Display(Description = "Organization đã vô hiệu hóa")]
    public const string Inactive = "98304";

    [Display(Description = "Dữ liệu organization không hợp lệ")]
    public const string InvalidInput = "98305";

    [Display(Description = "Catalog không xóa cứng organization")]
    public const string HardDeleteDisabled = "98306";
}
