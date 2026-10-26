using Platform.OrgStructure.Models;

namespace Platform.OrgStructure.Templates;

/// <summary>Tổng công ty: gốc tập đoàn, khối và phòng ban.</summary>
public static class CorporationTemplate
{
    public static OrgStructureTemplatePreview Preview(string rootName)
    {
        var name = rootName.Trim();
        var rootCode = OrgStructureLimits.NormalizeCode(name);
        if (rootCode.Length == 0 || rootCode.Length > OrgStructureLimits.CodeMaxLength)
            throw new ArgumentException("Tên gốc của mẫu không tạo được mã.", nameof(rootName));

        var rootPath = "/" + rootCode;
        var dieuHanh = "khoi-dieu-hanh";
        var kinhDoanh = "khoi-kinh-doanh";
        var taiChinh = "khoi-tai-chinh";
        OrgStructureTemplateNode[] nodes =
        [
            new(rootCode, name, SuggestedUnitTypes.Group, null, rootPath),
            new(dieuHanh, "Khối điều hành", SuggestedUnitTypes.Division, rootCode, rootPath + "/" + dieuHanh),
            new(kinhDoanh, "Khối kinh doanh", SuggestedUnitTypes.Division, rootCode, rootPath + "/" + kinhDoanh),
            new(taiChinh, "Khối tài chính", SuggestedUnitTypes.Division, rootCode, rootPath + "/" + taiChinh),
            Dept("phong-ke-toan", "Phòng kế toán", dieuHanh, rootPath + "/" + dieuHanh),
            Dept("phong-nhan-su", "Phòng nhân sự", dieuHanh, rootPath + "/" + dieuHanh),
            Dept("phong-ban-hang", "Phòng bán hàng", kinhDoanh, rootPath + "/" + kinhDoanh),
            Dept("phong-dich-vu", "Phòng dịch vụ", kinhDoanh, rootPath + "/" + kinhDoanh),
            Dept("phong-ngan-sach", "Phòng ngân sách", taiChinh, rootPath + "/" + taiChinh),
        ];
        return new OrgStructureTemplatePreview(OrgStructureLimits.CorporationTemplate, name, nodes);
    }

    private static OrgStructureTemplateNode Dept(string code, string label, string parentCode, string parentPath) =>
        new(code, label, SuggestedUnitTypes.Department, parentCode, parentPath + "/" + code);
}
