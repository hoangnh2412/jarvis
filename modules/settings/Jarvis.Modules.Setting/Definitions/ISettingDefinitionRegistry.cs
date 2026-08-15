namespace Jarvis.Modules.Setting.Definitions;

/// <summary>
/// Library cấu hình (in-memory): nguồn sự thật cho metadata Group/Key.
/// Không chứa giá trị runtime — Value nằm ở DB theo từng tenant.
/// </summary>
/// <remarks>
/// Dùng để:
/// <list type="bullet">
/// <item>UI lấy danh sách nhóm / definition dựng form.</item>
/// <item>Manager kiểm tra Key hợp lệ trước khi Create/Update.</item>
/// <item>Lấy DefaultValue, IsEncrypted, IsReadOnly từ code-first.</item>
/// </list>
/// </remarks>
public interface ISettingDefinitionRegistry
{
    /// <summary>Lấy toàn bộ nhóm, đã sắp xếp theo <c>Order</c> rồi <c>Name</c>.</summary>
    IReadOnlyList<SettingGroupDefinition> GetGroups();

    /// <summary>
    /// Lấy một nhóm theo tên.
    /// </summary>
    /// <param name="name">Mã nhóm (không phân biệt hoa thường).</param>
    /// <returns>Definition nhóm, hoặc null nếu chưa đăng ký.</returns>
    SettingGroupDefinition? GetGroup(string name);

    /// <summary>
    /// Lấy danh sách definition Key, có thể lọc theo Group.
    /// </summary>
    /// <param name="group">Mã nhóm; null/rỗng = lấy tất cả.</param>
    IReadOnlyList<SettingDefinition> GetSettings(string? group = null);

    /// <summary>
    /// Lấy definition theo Key.
    /// </summary>
    /// <param name="key">Mã cấu hình (không phân biệt hoa thường).</param>
    /// <returns>Definition, hoặc null nếu chưa đăng ký.</returns>
    SettingDefinition? GetSetting(string key);
}
