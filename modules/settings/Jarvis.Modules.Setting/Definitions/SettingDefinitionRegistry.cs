namespace Jarvis.Modules.Setting.Definitions;

/// <summary>
/// Registry Library mặc định: lưu Group/Setting trong dictionary bộ nhớ.
/// Được dựng lúc startup bằng cách gọi lần lượt mọi <see cref="ISettingDefinitionProvider"/>.
/// </summary>
/// <remarks>
/// Nghiệp vụ chính:
/// <list type="bullet">
/// <item>Phát hiện trùng Key ngay khi đăng ký (fail-fast lúc startup).</item>
/// <item>Type = Password → tự bật <c>IsEncrypted</c>.</item>
/// <item>Tra cứu không phân biệt hoa thường.</item>
/// </list>
/// </remarks>
public sealed class SettingDefinitionRegistry : ISettingDefinitionRegistry, ISettingDefinitionContext
{
    private readonly Dictionary<string, SettingGroupDefinition> _groups =
        new(StringComparer.OrdinalIgnoreCase);

    private readonly Dictionary<string, SettingDefinition> _settings =
        new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Tạo registry và nạp toàn bộ provider đã đăng ký DI.
    /// </summary>
    /// <param name="providers">Danh sách provider code-first từ host/module nghiệp vụ.</param>
    public SettingDefinitionRegistry(IEnumerable<ISettingDefinitionProvider> providers)
    {
        // Mỗi provider khai báo Group/Key của riêng mình; gọi tuần tự để fail sớm nếu trùng Key.
        foreach (var provider in providers)
            provider.Define(this);
    }

    public SettingGroupDefinition AddGroup(string name, Action<SettingGroupDefinition>? configure = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        // Group có thể được Add nhiều lần (từ nhiều provider) — lần sau chỉ cập nhật metadata.
        if (!_groups.TryGetValue(name, out var group))
        {
            group = new SettingGroupDefinition(name)
            {
                DisplayName = name,
            };
            _groups[name] = group;
        }

        configure?.Invoke(group);

        if (string.IsNullOrWhiteSpace(group.DisplayName))
            group.DisplayName = group.Name;

        return group;
    }

    public SettingDefinition AddSetting(string group, string key, Action<SettingDefinition>? configure = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(group);
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        // Đảm bảo Group tồn tại trước khi gắn Key.
        AddGroup(group);

        // Key trùng trong Library là lỗi cấu hình sản phẩm — dừng ngay lúc startup.
        if (_settings.ContainsKey(key))
            throw new InvalidOperationException($"Setting key '{key}' is already registered.");

        var definition = new SettingDefinition(group, key)
        {
            Name = key,
        };

        configure?.Invoke(definition);

        if (string.IsNullOrWhiteSpace(definition.Name))
            definition.Name = definition.Key;

        // Password mặc định phải mã hóa khi lưu; tránh quên set IsEncrypted ở provider.
        if (!definition.IsEncrypted &&
            string.Equals(definition.Type, SettingValueTypes.Password, StringComparison.OrdinalIgnoreCase))
        {
            definition.IsEncrypted = true;
        }

        _settings[key] = definition;
        return definition;
    }

    public IReadOnlyList<SettingGroupDefinition> GetGroups() =>
        _groups.Values.OrderBy(x => x.Order).ThenBy(x => x.Name, StringComparer.OrdinalIgnoreCase).ToList();

    public SettingGroupDefinition? GetGroup(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        return _groups.TryGetValue(name, out var group) ? group : null;
    }

    public IReadOnlyList<SettingDefinition> GetSettings(string? group = null)
    {
        IEnumerable<SettingDefinition> query = _settings.Values;

        if (!string.IsNullOrWhiteSpace(group))
            query = query.Where(x => string.Equals(x.Group, group, StringComparison.OrdinalIgnoreCase));

        return query.OrderBy(x => x.Group, StringComparer.OrdinalIgnoreCase)
            .ThenBy(x => x.Key, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    public SettingDefinition? GetSetting(string key)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        return _settings.TryGetValue(key, out var definition) ? definition : null;
    }
}
