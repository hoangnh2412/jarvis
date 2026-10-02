using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Platform.Workflow.Abstractions;
using Platform.Workflow.Idempotency;

namespace Platform.Workflow.Services;

public class RemoteWorkflowService(
    HttpClient httpClient,
    IWorkflowIdempotencyExecutor? idempotencyExecutor = null) : IWorkflowService
{
    public async Task<WorkflowExecutionResult> RunAsync(
        string workflowDefinitionId,
        IDictionary<string, object>? input = null,
        string? correlationId = null,
        string? idempotencyKey = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(workflowDefinitionId);

        if (!string.IsNullOrWhiteSpace(idempotencyKey) && idempotencyExecutor != null)
        {
            var cached = await idempotencyExecutor.ExecuteAsync(
                idempotencyKey,
                () => ExecuteRunAsync(workflowDefinitionId, input, correlationId, cancellationToken),
                cancellationToken: cancellationToken);

            return cached ?? new WorkflowExecutionResult(null, "Executed");
        }

        return await ExecuteRunAsync(workflowDefinitionId, input, correlationId, cancellationToken);
    }

    private async Task<WorkflowExecutionResult> ExecuteRunAsync(
        string workflowDefinitionId,
        IDictionary<string, object>? input,
        string? correlationId,
        CancellationToken cancellationToken)
    {
        var payload = new
        {
            DefinitionId = workflowDefinitionId,
            Input = input,
            CorrelationId = correlationId
        };

        using var response = await httpClient.PostAsJsonAsync(
            $"workflow-definitions/{Uri.EscapeDataString(workflowDefinitionId)}/execute",
            payload,
            cancellationToken);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<ElsaExecuteResponse>(cancellationToken: cancellationToken);
        return new WorkflowExecutionResult(
            result?.WorkflowInstanceId,
            result?.Status ?? "Executed",
            result?.Output,
            result?.IsFaulted ?? false,
            result?.FaultMessage);
    }

    public async Task<WorkflowExecutionResult> ResumeAsync(
        string workflowInstanceId,
        string? bookmarkId = null,
        IDictionary<string, object>? input = null,
        string? idempotencyKey = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(workflowInstanceId);

        if (!string.IsNullOrWhiteSpace(idempotencyKey) && idempotencyExecutor != null)
        {
            var cached = await idempotencyExecutor.ExecuteAsync(
                idempotencyKey,
                () => ExecuteResumeAsync(workflowInstanceId, bookmarkId, input, cancellationToken),
                cancellationToken: cancellationToken);

            return cached ?? new WorkflowExecutionResult(workflowInstanceId, "Resumed");
        }

        return await ExecuteResumeAsync(workflowInstanceId, bookmarkId, input, cancellationToken);
    }

    private async Task<WorkflowExecutionResult> ExecuteResumeAsync(
        string workflowInstanceId,
        string? bookmarkId,
        IDictionary<string, object>? input,
        CancellationToken cancellationToken)
    {
        var payload = new
        {
            WorkflowInstanceId = workflowInstanceId,
            BookmarkId = bookmarkId,
            Input = input
        };

        using var response = await httpClient.PostAsJsonAsync(
            $"workflow-instances/{Uri.EscapeDataString(workflowInstanceId)}/resume",
            payload,
            cancellationToken);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<ElsaExecuteResponse>(cancellationToken: cancellationToken);
        return new WorkflowExecutionResult(
            result?.WorkflowInstanceId ?? workflowInstanceId,
            result?.Status ?? "Resumed",
            result?.Output,
            result?.IsFaulted ?? false,
            result?.FaultMessage);
    }

    private sealed record ElsaExecuteResponse(
        [property: JsonPropertyName("workflowInstanceId")] string? WorkflowInstanceId,
        [property: JsonPropertyName("status")] string? Status,
        [property: JsonPropertyName("output")] IDictionary<string, object>? Output,
        [property: JsonPropertyName("isFaulted")] bool? IsFaulted,
        [property: JsonPropertyName("faultMessage")] string? FaultMessage
    );
}
