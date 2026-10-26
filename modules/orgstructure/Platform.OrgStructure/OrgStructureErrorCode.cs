using System.ComponentModel.DataAnnotations;
using Platform.DDD.Domain.Shared.ExceptionHandling;

namespace Platform.OrgStructure;

/// <summary>Mã lỗi cây đơn vị trong một Org.</summary>
public class OrgStructureErrorCode : IErrorCode
{
    [Display(Description = "Không tìm thấy đơn vị hoặc danh mục liên quan")]
    public const string NotFound = "98501";

    [Display(Description = "Mã đã tồn tại trong Org")]
    public const string CodeAlreadyExists = "98502";

    [Display(Description = "Dữ liệu đơn vị không hợp lệ")]
    public const string InvalidInput = "98503";

    [Display(Description = "Đơn vị cha không hợp lệ hoặc tạo vòng")]
    public const string InvalidParent = "98504";

    [Display(Description = "Đơn vị cha đã vô hiệu hóa")]
    public const string ParentInactive = "98505";

    [Display(Description = "Đơn vị còn vị trí đang hoạt động")]
    public const string HasActivePositions = "98506";

    [Display(Description = "Pháp nhân không thuộc Org hoặc đã vô hiệu hóa")]
    public const string InvalidLegalEntity = "98507";

    [Display(Description = "Vị trí báo cáo không hợp lệ hoặc tạo vòng")]
    public const string InvalidReportsTo = "98508";

    [Display(Description = "Org đã có đơn vị, không áp mẫu lên cây sẵn có")]
    public const string TemplateNotEmpty = "98509";
}
