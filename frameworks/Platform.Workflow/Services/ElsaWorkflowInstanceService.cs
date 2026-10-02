using Elsa.Workflows.Management;
using Elsa.Workflows.Management.Filters;
using Platform.Workflow.Abstractions;
using ICoreWorkflowInstanceService = Platform.Workflow.Abstractions.IWorkflowInstanceService;

namespace Platform.Workflow.Services;

public class ElsaWorkflowInstanceService(
    IWorkflowInstanceStore workflowInstanceStore) : ICoreWorkflowInstanceService
{
    public async Task<IReadOnlyCollection<string>> ListAsync(CancellationToken cancellationToken = default)
    {
        var filter = new WorkflowInstanceFilter();
        var summaries = await workflowInstanceStore.SummarizeManyAsync(
            filter,
            cancellationToken);

        return summaries.Select(x => x.Id).Distinct().ToList();
    }

    public async Task<WorkflowInstanceMetadata?> GetAsync(string instanceId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(instanceId);

        var instance = await workflowInstanceStore.FindAsync(
            new WorkflowInstanceFilter { Id = instanceId },
            cancellationToken);

        return instance == null
            ? null
            : new WorkflowInstanceMetadata(
                instance.Id,
                instance.DefinitionId,
                instance.Status.ToString(),
                instance.CreatedAt,
                instance.FinishedAt);
    }

    public async Task<bool> DeleteAsync(string instanceId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(instanceId);

        var deletedCount = await workflowInstanceStore.DeleteAsync(
            new WorkflowInstanceFilter { Id = instanceId },
            cancellationToken);

        return deletedCount > 0;
    }
}
