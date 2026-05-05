namespace Jarvis.HealthChecks;

/// <summary>
/// Signals when expensive startup work finished (migrations, cache warm-up); drives the startup probe only.
/// Call <see cref="MarkStartupComplete"/> after initialization (e.g. after <c>EnsureMigrateDb</c>).
/// Báo hiệu khi bước khởi động nặng đã xong (migration, warm-up cache); chỉ phục vụ probe startup.
/// Gọi <see cref="MarkStartupComplete"/> sau khởi tạo (vd. sau <c>EnsureMigrateDb</c>).
/// </summary>
public interface IStartupCompletionNotifier
{
    /// <summary>
    /// True after <see cref="MarkStartupComplete"/> was invoked.
    /// True sau khi đã gọi <see cref="MarkStartupComplete"/>.
    /// </summary>
    bool IsStartupComplete { get; }

    /// <summary>
    /// Marks startup phase complete so <c>/health/startup</c> can succeed.
    /// Đánh dấu giai đoạn startup hoàn tất để <c>/health/startup</c> có thể thành công.
    /// </summary>
    void MarkStartupComplete();
}
