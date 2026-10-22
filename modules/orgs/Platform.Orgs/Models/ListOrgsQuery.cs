namespace Platform.Orgs.Models;

/// <summary>Bộ lọc list Org trong tenant đang làm việc.</summary>
public sealed class ListOrgsQuery
{
    public bool? IsActive { get; set; }

    public string? Search { get; set; }
}
