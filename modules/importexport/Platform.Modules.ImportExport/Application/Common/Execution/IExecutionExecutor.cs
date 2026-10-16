using Platform.Modules.ImportExport.Domain.Enums;

namespace Platform.Modules.ImportExport.Application.Common.Execution;

/// <summary>Execution seam: GD2 will add a job-backed executor without changing processors or public facade methods.</summary>
public interface IExecutionExecutor
{
    Task<TResult> ExecuteAsync<TResult>(ExecutionMode mode, Func<CancellationToken, Task<TResult>> operation, CancellationToken cancellationToken);
}