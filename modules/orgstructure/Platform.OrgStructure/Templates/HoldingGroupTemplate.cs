using Platform.OrgStructure.Models;

namespace Platform.OrgStructure.Templates;

/// <summary>Tập đoàn: gốc tập đoàn, công ty thành viên và phòng ban.</summary>
public static class HoldingGroupTemplate
{
    public static OrgStructureTemplatePreview Preview(string rootName)
    {
        var name = rootName.Trim();
        var rootCode = OrgStructureLimits.NormalizeCode(name);
        if (rootCode.Length == 0 || rootCode.Length > OrgStructureLimits.CodeMaxLength)
            throw new ArgumentException("Tên gốc của mẫu không tạo được mã.", nameof(rootName));

        var rootPath = "/" + rootCode;
        var companyA = "cong-ty-a";
        var companyB = "cong-ty-b";
        OrgStructureTemplateNode[] nodes =
        [
            new(rootCode, name, SuggestedUnitTypes.Group, null, rootPath),
            new(companyA, "Công ty thành viên A", SuggestedUnitTypes.Company, rootCode, rootPath + "/" + companyA),
            new(companyB, "Công ty thành viên B", SuggestedUnitTypes.Company, rootCode, rootPath + "/" + companyB),
            Dept("a-ke-toan", "Phòng kế toán", companyA, rootPath + "/" + companyA),
            Dept("a-kinh-doanh", "Phòng kinh doanh", companyA, rootPath + "/" + companyA),
            Dept("b-ke-toan", "Phòng kế toán", companyB, rootPath + "/" + companyB),
            Dept("b-van-hanh", "Phòng vận hành", companyB, rootPath + "/" + companyB),
        ];
        return new OrgStructureTemplatePreview(OrgStructureLimits.HoldingGroupTemplate, name, nodes);
    }

    private static OrgStructureTemplateNode Dept(string code, string label, string parentCode, string parentPath) =>
        new(code, label, SuggestedUnitTypes.Department, parentCode, parentPath + "/" + code);
}
