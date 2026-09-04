using System.ComponentModel.DataAnnotations;
using Jarvis.DDD.Domain.Shared.ExceptionHandling;

namespace Jarvis.Modules.Setting;

/// <summary>
/// Business error codes for the Setting module.
/// </summary>
public class SettingErrorCode : IErrorCode
{
    [Display(Description = "Không tìm thấy cấu hình theo mã với khách hàng hiện tại")]
    public const string NotFound = "98101";

    [Display(Description = "Mã hoặc nhóm không có trong danh mục cấu hình có sẵn")]
    public const string DefinitionNotFound = "98102";

    [Display(Description = "Mã cấu hình đã tồn tại")]
    public const string KeyAlreadyExists = "98103";

    [Display(Description = "Cấu hình chỉ đọc — không được sửa/xóa")]
    public const string ReadOnly = "98104";

    [Display(Description = "Hệ thống chưa xác định được khách hàng đang thao tác")]
    public const string TenantRequired = "98105";

    [Display(Description = "Khóa mã hóa thiếu hoặc sai cấu hình")]
    public const string EncryptionKeyInvalid = "98106";

    [Display(Description = "Giá trị cấu hình không hợp lệ theo kiểu đã định nghĩa")]
    public const string InvalidValue = "98107";
}
