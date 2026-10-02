using System.Net.Http.Json;
using Platform.Workflow.Abstractions;

namespace Platform.Workflow.Services;

public class RemoteWorkflowInstanceService(HttpClient httpClient) : IWorkflowInstanceService
{
    public async Task<IReadOnlyCollection<string>> ListAsync(CancellationToken cancellationToken = default)
    {
        var response = await httpClient.GetFromJsonAsync<List<string>>("workflow-instances", cancellationToken);
        return response ?? [];
    }

    public async Task<WorkflowInstanceMetadata?> GetAsync(string instanceId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(instanceId);

        try
        {
            var response = await httpClient.GetFromJsonAsync<WorkflowInstanceDto>(
                $"workflow-instances/{Uri.EscapeDataString(instanceId)}", 
                cancellationToken);

            return response == null ? null : new WorkflowInstanceMetadata(
                response.Id,
                response.DefinitionId,
                response.Status,
                response.CreatedAt,
                response.CompletedAt);
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task<bool> DeleteAsync(string instanceId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(instanceId);

        using var response = await httpClient.DeleteAsync(
            $"workflow-instances/{Uri.EscapeDataString(instanceId)}", 
            cancellationToken);

        return response.IsSuccessStatusCode;
    }

    private sealed record WorkflowInstanceDto(
        string Id,
        string? DefinitionId,
        string? Status,
        DateTimeOffset? CreatedAt,
        DateTimeOffset? CompletedAt);
}
