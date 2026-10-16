namespace Platform.ImportExport.MiniExcel.Helpers;

/// <summary>
/// Soft timeout chá»“ng lÃªn <see cref="CancellationToken"/> cá»§a caller (RISK-001).
/// </summary>
/// <remarks>
/// DÃ¹ng bá»Ÿi <c>MiniExcelImportProcessor</c> / <c>MiniExcelExportProcessor</c>.
/// Timeout chÃ¡y trÆ°á»›c abort client â†’ <see cref="OperationTimeoutException"/> (host map HTTP 408);
/// client abort â†’ giá»¯ <see cref="OperationCanceledException"/> gá»‘c.
/// </remarks>
internal static class OperationTimeoutScope
{
    /// <summary>
    /// Runs <paramref name="action"/> with a linked token that cancels after <paramref name="timeoutSeconds"/>.
    /// When <paramref name="timeoutSeconds"/> â‰¤ 0, only <paramref name="callerToken"/> is used.
    /// </summary>
    /// <exception cref="OperationTimeoutException">Thrown when the soft timeout fires first.</exception>
    public static async Task<T> RunAsync<T>(
        int timeoutSeconds,
        string operationName,
        CancellationToken callerToken,
        Func<CancellationToken, Task<T>> action)
    {
        if (timeoutSeconds <= 0)
            return await action(callerToken);

        using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(timeoutSeconds));
        using var linked = CancellationTokenSource.CreateLinkedTokenSource(callerToken, timeoutCts.Token);

        try
        {
            return await action(linked.Token);
        }
        catch (OperationCanceledException) when (
            timeoutCts.IsCancellationRequested && !callerToken.IsCancellationRequested)
        {
            throw new OperationTimeoutException(operationName, timeoutSeconds);
        }
    }

    /// <summary>Non-generic overload for fire-and-forget style async work.</summary>
    public static async Task RunAsync(
        int timeoutSeconds,
        string operationName,
        CancellationToken callerToken,
        Func<CancellationToken, Task> action) =>
        await RunAsync(timeoutSeconds, operationName, callerToken, async ct =>
        {
            await action(ct);
            return true;
        });
}
