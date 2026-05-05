namespace Jarvis.HealthChecks;

/// <summary>
/// Thread-safe flag implementation of <see cref="IStartupCompletionNotifier"/> using volatile int.
/// Triển khai cờ an toàn luồng cho <see cref="IStartupCompletionNotifier"/> dùng volatile int.
/// </summary>
internal sealed class StartupCompletionNotifier : IStartupCompletionNotifier
{
    // EN: 0 = not complete, 1 = complete / VI: 0 = chưa xong, 1 = đã xong
    private int _complete;

    /// <inheritdoc />
    public bool IsStartupComplete => Volatile.Read(ref _complete) != 0;

    /// <inheritdoc />
    public void MarkStartupComplete() => Volatile.Write(ref _complete, 1);
}
