namespace Platform.Orgs.Models;

/// <summary>
/// Input cập nhật Org (cột chuẩn). Host kế thừa class này để truyền thêm field cho hooks.
/// </summary>
public class UpdateOrgInput
{
    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;
}
