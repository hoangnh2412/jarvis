namespace Jarvis.Modules.Setting.Definitions;

/// <summary>
/// Context fluent dùng trong <see cref="ISettingDefinitionProvider.Define"/> để đăng ký danh mục cấu hình.
/// </summary>
public interface ISettingDefinitionContext
{
    /// <summary>
    /// Thêm hoặc cập nhật metadata một Group trong Library.
    /// Group chỉ tồn tại trong bộ nhớ — không lưu bảng DB.
    /// </summary>
    /// <param name="name">Mã nhóm (ví dụ <c>Email</c>). Khớp với <c>ISettingEntity.Group</c>.</param>
    /// <param name="configure">Tuỳ chỉnh DisplayName, Description, Order…</param>
    /// <returns>Định nghĩa nhóm sau khi cấu hình.</returns>
    SettingGroupDefinition AddGroup(string name, Action<SettingGroupDefinition>? configure = null);

    /// <summary>
    /// Đăng ký một Key cấu hình thuộc Group.
    /// Key phải duy nhất trong toàn bộ Library (không phân biệt hoa thường).
    /// </summary>
    /// <param name="group">Mã nhóm sở hữu Key. Group sẽ được tạo tự động nếu chưa có.</param>
    /// <param name="key">Mã cấu hình (ví dụ <c>Email.Host</c>).</param>
    /// <param name="configure">Tuỳ chỉnh Name, Type, Options, DefaultValue, IsReadOnly, IsEncrypted…</param>
    /// <returns>Định nghĩa Key sau khi cấu hình.</returns>
    /// <exception cref="InvalidOperationException">Key đã được đăng ký trước đó.</exception>
    SettingDefinition AddSetting(string group, string key, Action<SettingDefinition>? configure = null);
}
