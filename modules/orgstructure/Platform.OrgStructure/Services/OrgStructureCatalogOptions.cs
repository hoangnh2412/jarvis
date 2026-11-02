namespace Platform.OrgStructure.Services;

public sealed record OrgUnitTypeDefault(string Code, string Name, string Icon);

public sealed record OrgCatalogDefault(string Code, string Name);

/// <summary>
/// Dữ liệu mặc định do host khai báo (tên/code đặc thù sản phẩm); khi Org chưa có dòng danh mục,
/// service seed các mục này. Icon ngoài danh sách cho phép bị đưa về <c>circle</c>.
/// </summary>
public sealed class OrgStructureCatalogOptions
{
    public IReadOnlyList<OrgUnitTypeDefault> UnitTypes { get; set; } = [];

    public IReadOnlyList<OrgCatalogDefault> JobGrades { get; set; } = [];

    public IReadOnlyList<OrgCatalogDefault> JobPositions { get; set; } = [];

    public IReadOnlyList<OrgCatalogDefault> JobTitles { get; set; } = [];

    public IReadOnlyList<string> AllowedIcons { get; set; } =
    [
        "building-2", "building", "landmark", "map-pinned", "git-branch", "layers-3",
        "briefcase", "layout-grid", "users", "circle", "network", "factory", "store", "warehouse",
    ];

    public string NormalizeIcon(string? icon)
    {
        var key = (icon ?? string.Empty).Trim().ToLowerInvariant();
        return AllowedIcons.Contains(key, StringComparer.OrdinalIgnoreCase) ? key : "circle";
    }
}
