namespace Platform.Modules.ImportExport.Domain.Enums;

/// <summary>
/// Cách chạy import/export. GĐ1: chỉ <see cref="Sync"/>. <see cref="Async"/> dành cho job queue (GĐ2).
/// </summary>
public enum ExecutionMode
{
    /// <summary>Chạy đồng bộ trên request hiện tại.</summary>
    Sync = 0,

    /// <summary>Chạy nền (chưa implement GĐ1).</summary>
    Async = 1
}