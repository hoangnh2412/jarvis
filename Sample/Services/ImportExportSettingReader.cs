using Platform.Modules.Setting.Services;
using Sample.Settings;

namespace Sample.Services;

/// <summary>
/// Reads ImportExport template code mappings from persisted Platform Setting values.
/// Add new modules by defining keys in <see cref="ImportExportSettingDefinition"/> only.
/// </summary>
public sealed class ImportExportSettingReader(ISettingManager settingManager)
{
    public Task<string?> GetTemplateCodeAsync(string key, CancellationToken cancellationToken = default) =>
        GetValueAsync(key, cancellationToken);

    public async Task<string> RequireTemplateCodeAsync(
        string key,
        CancellationToken cancellationToken = default) =>
        await GetTemplateCodeAsync(key, cancellationToken).ConfigureAwait(false)
        ?? throw new InvalidOperationException(
            $"Setting '{key}' is missing or empty. " +
            $"Configure it via PUT /api/v1/settings/group/{ImportExportSettingDefinition.ImportGroupName} " +
            $"or PUT /api/v1/settings/group/{ImportExportSettingDefinition.ExportGroupName}.");

    private async Task<string?> GetValueAsync(string key, CancellationToken cancellationToken)
    {
        var setting = await settingManager.GetAsync(key, cancellationToken).ConfigureAwait(false);
        var value = setting?.Value?.Trim();
        return string.IsNullOrWhiteSpace(value) ? null : value;
    }
}
