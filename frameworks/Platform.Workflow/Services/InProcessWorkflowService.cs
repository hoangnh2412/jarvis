using Elsa.Common.Models;
using Elsa.Workflows;
using Elsa.Workflows.Management;
using Elsa.Workflows.Options;
using Elsa.Workflows.Runtime;
using Elsa.Workflows.Runtime.Messages;
using Platform.Workflow.Abstractions;
using Platform.Workflow.Idempotency;
using IElsaWorkflowDefinitionService = Elsa.Workflows.Management.IWorkflowDefinitionService;

namespace Platform.Workflow.Services;

public class InProcessWorkflowService(
    IElsaWorkflowDefinitionService workflowDefinitionService,
    IWorkflowRunner workflowRunner,
    IWorkflowRuntime workflowRuntime,
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

            return cached ?? new WorkflowExecutionResult(null, "Completed");
        }

        return await ExecuteRunAsync(workflowDefinitionId, input, correlationId, cancellationToken);
    }

    private async Task<WorkflowExecutionResult> ExecuteRunAsync(
        string workflowDefinitionId,
        IDictionary<string, object>? input,
        string? correlationId,
        CancellationToken cancellationToken)
    {
        var graph = await workflowDefinitionService.FindWorkflowGraphAsync(workflowDefinitionId, VersionOptions.Published, cancellationToken: cancellationToken);
        if (graph == null)
        {
            throw new InvalidOperationException($"Workflow definition '{workflowDefinitionId}' was not found.");
        }

        var runOptions = new RunWorkflowOptions
        {
            Input = input,
            CorrelationId = correlationId
        };

        var result = await workflowRunner.RunAsync(graph.Workflow, runOptions, cancellationToken: cancellationToken);
        var isFaulted = result.WorkflowState.Status == WorkflowStatus.Finished && result.WorkflowState.SubStatus == WorkflowSubStatus.Faulted;
        var faultMessage = isFaulted ? result.WorkflowState.Incidents.FirstOrDefault()?.Message : null;

        return new WorkflowExecutionResult(
            result.WorkflowState.Id,
            result.WorkflowState.Status.ToString(),
            result.WorkflowState.Output,
            isFaulted,
            faultMessage);
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
        var client = await workflowRuntime.CreateClientAsync(workflowInstanceId, cancellationToken);
        if (!await client.InstanceExistsAsync(cancellationToken))
        {
            throw new InvalidOperationException($"Workflow instance '{workflowInstanceId}' was not found.");
        }

        var request = new RunWorkflowInstanceRequest
        {
            BookmarkId = bookmarkId,
            Input = input
        };

        var response = await client.RunInstanceAsync(request, cancellationToken);
        var isFaulted = response.Status == WorkflowStatus.Finished && response.SubStatus == WorkflowSubStatus.Faulted;
        var faultMessage = isFaulted ? response.Incidents.FirstOrDefault()?.Message : null;

        return new WorkflowExecutionResult(
            response.WorkflowInstanceId,
            response.Status.ToString(),
            response.Output,
            isFaulted,
            faultMessage);
    }
}
