using Platform.Tenants.Models;

namespace Platform.Orgs.Models;

/// <summary>
/// Input tạo Org (cột chuẩn). Host kế thừa class này để truyền thêm field cho hooks.
/// </summary>
public class CreateOrgInput
{
    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public Guid? ParentId { get; set; }

    public CatalogAdminAccountInput? InitialAdmin { get; set; }
}
