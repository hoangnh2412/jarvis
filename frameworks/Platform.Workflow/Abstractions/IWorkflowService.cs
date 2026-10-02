namespace Platform.Workflow.Abstractions;

public interface IWorkflowService
{
    Task<WorkflowExecutionResult> RunAsync(
        string workflowDefinitionId,
        IDictionary<string, object>? input = null,
        string? correlationId = null,
        string? idempotencyKey = null,
        CancellationToken cancellationToken = default);

    Task<WorkflowExecutionResult> ResumeAsync(
        string workflowInstanceId,
        string? bookmarkId = null,
        IDictionary<string, object>? input = null,
        string? idempotencyKey = null,
        CancellationToken cancellationToken = default);
}

public sealed record WorkflowExecutionResult(
    string? WorkflowInstanceId,
    string? Status,
    IDictionary<string, object>? Output = null,
    bool IsFaulted = false,
    string? FaultMessage = null);
