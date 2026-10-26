using Platform.OrgStructure.Models;

namespace Platform.OrgStructure.Templates;

/// <summary>Mẫu cố định. Không ghi dữ liệu.</summary>
public static class SimpleCompanyTemplate
{
    public static OrgStructureTemplatePreview Preview(string rootName)
    {
        var name = rootName.Trim();
        var rootCode = OrgStructureLimits.NormalizeCode(name);
        if (rootCode.Length == 0 || rootCode.Length > OrgStructureLimits.CodeMaxLength)
            throw new ArgumentException("Tên gốc của mẫu không tạo được mã.", nameof(rootName));

        var rootPath = "/" + rootCode;
        OrgStructureTemplateNode[] nodes =
        [
            new(rootCode, name, SuggestedUnitTypes.Company, null, rootPath),
            Child("ke-toan", "Kế toán", rootCode, rootPath),
            Child("kinh-doanh", "Kinh doanh", rootCode, rootPath),
            Child("nhan-su", "Nhân sự", rootCode, rootPath),
        ];
        return new OrgStructureTemplatePreview(OrgStructureLimits.SimpleCompanyTemplate, name, nodes);
    }

    private static OrgStructureTemplateNode Child(string code, string name, string parentCode, string parentPath) =>
        new(code, name, SuggestedUnitTypes.Department, parentCode, parentPath + "/" + code);
}
