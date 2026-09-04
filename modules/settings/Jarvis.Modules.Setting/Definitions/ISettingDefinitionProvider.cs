namespace Jarvis.Modules.Setting.Definitions;

/// <summary>
/// Điểm mở rộng code-first để host/module nghiệp vụ khai báo danh mục Group và Key.
/// </summary>
/// <remarks>
/// Cách dùng:
/// <list type="number">
/// <item>Tạo class implement interface này, gọi <c>AddGroup</c>/<c>AddSetting</c> trong <see cref="Define"/>.</item>
/// <item>Đăng ký qua <c>AddCoreSetting(...).AddProvider&lt;TProvider&gt;()</c>.</item>
/// <item>Registry gọi <see cref="Define"/> một lần lúc startup để dựng Library trong bộ nhớ.</item>
/// </list>
/// Provider chỉ khai báo metadata — không đọc/ghi giá trị runtime (Value do <c>ISettingManager</c> quản lý).
/// </remarks>
public interface ISettingDefinitionProvider
{
    /// <summary>
    /// Đăng ký Group và Setting vào Library.
    /// Được gọi đúng một lần khi khởi tạo <see cref="SettingDefinitionRegistry"/>.
    /// </summary>
    /// <param name="context">Context fluent để thêm Group/Setting.</param>
    void Define(ISettingDefinitionContext context);
}
