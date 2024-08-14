namespace Jarvis.Domain.Common.Interfaces;

/// <summary>
/// The interface abstracts output pagination parameters
/// </summary>
public interface IPaging
{
    /// <summary>
    /// Số bản ghi trên 1 trang
    /// </summary>
    int Size { get; set; }

    /// <summary>
    /// Số trang hiện tại
    /// </summary>
    int Page { get; set; }

    /// <summary>
    /// Từ khóa tìm kiếm
    /// </summary>
    string Q { get; set; }

    /// <summary>
    /// Các cột tìm kiếm
    /// </summary>
    Dictionary<string, string> Filters { get; set; }

    /// <summary>
    /// Các cột sắp xếp
    /// </summary>
    Dictionary<string, string> Sort { get; set; }

    /// <summary>
    /// Các cột hiển thị, trả theo thứ tự cột
    /// </summary>
    IEnumerable<string> Columns { get; set; }
}