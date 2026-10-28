namespace Platform.OrgStructure;

/// <summary>Cấu hình cây đơn vị. <see cref="FilterABACByOrgStructure"/> mặc định tắt.</summary>
public sealed class OrgStructureOptions
{
    public const string SectionName = "OrgStructure";

    public bool FilterABACByOrgStructure { get; set; }
}
