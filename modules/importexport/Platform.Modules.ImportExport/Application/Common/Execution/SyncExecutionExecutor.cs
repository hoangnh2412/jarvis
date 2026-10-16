using Platform.Modules.ImportExport.Domain.Constants;
using Platform.Modules.ImportExport.Domain.Enums;

namespace Platform.Modules.ImportExport.Application.Common.Execution;

/// <summary>
/// Executor GĐ1: chỉ hỗ trợ <see cref="ExecutionMode.Sync"/>.
/// </summary>
public sealed class SyncExecutionExecutor : IExecutionExecutor
{
    public Task<TResult> ExecuteAsync<TResult>(
        ExecutionMode mode,
        Func<CancellationToken, Task<TResult>> operation,
        CancellationToken cancellationToken)
    {
        if (mode is not ExecutionMode.Sync)
            throw new NotSupportedException(EngineMessages.AsyncModeNotSupported);

        return operation(cancellationToken);
    }
}
