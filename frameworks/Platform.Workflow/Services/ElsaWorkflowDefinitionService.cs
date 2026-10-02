using System.Text.Json;
using Elsa.Common.Models;
using Elsa.Workflows.Management;
using Elsa.Workflows.Management.Filters;
using Platform.Workflow.Abstractions;
using ICoreWorkflowDefinitionService = Platform.Workflow.Abstractions.IWorkflowDefinitionService;

namespace Platform.Workflow.Services;

public class ElsaWorkflowDefinitionService(
    IWorkflowDefinitionStore workflowDefinitionStore) : ICoreWorkflowDefinitionService
{
    public async Task<IReadOnlyCollection<string>> ListAsync(CancellationToken cancellationToken = default)
    {
        var summaries = await workflowDefinitionStore.FindSummariesAsync(
            new WorkflowDefinitionFilter(),
            cancellationToken);

        return summaries.Select(x => x.DefinitionId).Distinct().ToList();
    }

    public async Task<WorkflowDefinitionMetadata?> GetAsync(string definitionId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(definitionId);

        var definition = await workflowDefinitionStore.FindAsync(
            new WorkflowDefinitionFilter { DefinitionId = definitionId },
            cancellationToken);

        return definition == null
            ? null
            : new WorkflowDefinitionMetadata(
                definition.DefinitionId,
                definition.Name,
                definition.Description,
                definition.Version,
                definition.IsPublished);
    }

    public async Task<WorkflowDefinitionDocument?> GetDocumentAsync(
        string definitionId,
        int? version = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(definitionId);

        var versionOptions = version is int value
            ? VersionOptions.SpecificVersion(value)
            : VersionOptions.Published;

        var definition = await workflowDefinitionStore.FindAsync(
            new WorkflowDefinitionFilter
            {
                DefinitionId = definitionId,
                VersionOptions = versionOptions
            },
            cancellationToken);

        if (definition == null && version == null)
        {
            definition = await workflowDefinitionStore.FindAsync(
                new WorkflowDefinitionFilter
                {
                    DefinitionId = definitionId,
                    VersionOptions = VersionOptions.Latest
                },
                cancellationToken);
        }

        if (definition == null)
            return null;

        var source = definition.OriginalSource ?? definition.StringData;
        object? document = null;
        if (!string.IsNullOrWhiteSpace(source))
        {
            using var jsonDocument = JsonDocument.Parse(source);
            document = jsonDocument.RootElement.Clone();
        }

        return new WorkflowDefinitionDocument(
            definition.DefinitionId,
            definition.Name,
            definition.Description,
            definition.Version,
            definition.IsPublished,
            definition.IsLatest,
            document);
    }

    public async Task<bool> DeleteAsync(string definitionId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(definitionId);

        var deletedCount = await workflowDefinitionStore.DeleteAsync(
            new WorkflowDefinitionFilter { DefinitionId = definitionId },
            cancellationToken);

        return deletedCount > 0;
    }
}
