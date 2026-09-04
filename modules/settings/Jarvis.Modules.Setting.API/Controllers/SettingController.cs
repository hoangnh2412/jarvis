using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Jarvis.Modules.Setting.API.Models;
using Jarvis.Modules.Setting.Services;

namespace Jarvis.Modules.Setting.API.Controllers;

/// <summary>
/// HTTP API chuẩn cho Setting metadata (Library) và giá trị đã persist.
/// Đăng ký qua <c>AddCoreSetting(...).UseHttpApi()</c> — không kèm Authorize (host nội bộ tự bảo vệ nếu cần).
/// Tenant do <c>ICurrentTenant&lt;TTenant&gt;</c> của host (middleware / R2) thiết lập trước khi vào action.
/// </summary>
[ApiVersionNeutral]
[Route("api/v{version:apiVersion}/settings")]
[ApiController]
public sealed class SettingController(ISettingManager settingManager) : ControllerBase
{
    /// <summary>Danh sách nhóm cấu hình (menu / tab).</summary>
    [HttpGet("groups")]
    public IActionResult GetGroups() => Ok(settingManager.GetGroups());

    /// <summary>Metadata Library (không kèm Value runtime).</summary>
    [HttpGet("definitions")]
    public IActionResult GetDefinitions([FromQuery] string? group = null) =>
        Ok(settingManager.GetDefinitions(group));

    /// <summary>
    /// Một call cho UI: definitions của group ⊕ giá trị hiện tại (hoặc default).
    /// </summary>
    [HttpGet("form")]
    public async Task<IActionResult> GetFormAsync(
        [FromQuery] string group,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(group))
            return BadRequest(new { message = "Query parameter 'group' is required." });

        var form = await settingManager.GetFormAsync(group.Trim(), cancellationToken).ConfigureAwait(false);
        return Ok(form);
    }

    /// <summary>Giá trị đã lưu DB theo group (có thể rỗng nếu chưa persist).</summary>
    [HttpGet]
    public async Task<IActionResult> GetByGroupAsync(
        [FromQuery] string group,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(group))
            return BadRequest(new { message = "Query parameter 'group' is required." });

        var settings = await settingManager.GetByGroupAsync(group.Trim(), cancellationToken).ConfigureAwait(false);
        return Ok(settings);
    }

    /// <summary>Lưu cả form group trong một request (upsert tất cả key).</summary>
    [HttpPut("group/{group}")]
    public async Task<IActionResult> SaveGroupAsync(
        string group,
        [FromBody] SettingGroupSaveRequest request,
        CancellationToken cancellationToken)
    {
        var settings = await settingManager
            .SaveGroupAsync(group, request.Values, cancellationToken)
            .ConfigureAwait(false);
        return Ok(settings);
    }

    /// <summary>Đọc một setting theo Key.</summary>
    [HttpGet("{key}")]
    public async Task<IActionResult> GetAsync(
        string key,
        CancellationToken cancellationToken)
    {
        var setting = await settingManager.GetAsync(key, cancellationToken).ConfigureAwait(false);
        return setting is null ? NotFound() : Ok(setting);
    }

    /// <summary>Tạo một row theo Key.</summary>
    [HttpPost("{key}")]
    public async Task<IActionResult> CreateAsync(
        string key,
        [FromBody] SettingValueRequest? request,
        CancellationToken cancellationToken)
    {
        var setting = await settingManager.CreateAsync(key, request?.Value, cancellationToken).ConfigureAwait(false);
        return Ok(setting);
    }

    /// <summary>Cập nhật Value theo Key.</summary>
    [HttpPut("{key}")]
    public async Task<IActionResult> UpdateAsync(
        string key,
        [FromBody] SettingValueRequest request,
        CancellationToken cancellationToken)
    {
        var setting = await settingManager.UpdateAsync(key, request.Value, cancellationToken).ConfigureAwait(false);
        return Ok(setting);
    }

    /// <summary>Xóa một setting theo Key.</summary>
    [HttpDelete("{key}")]
    public async Task<IActionResult> DeleteAsync(
        string key,
        CancellationToken cancellationToken)
    {
        await settingManager.DeleteAsync(key, cancellationToken).ConfigureAwait(false);
        return NoContent();
    }
}
