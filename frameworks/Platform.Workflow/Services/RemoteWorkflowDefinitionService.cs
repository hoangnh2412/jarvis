using System.Net.Http.Json;
using System.Text.Json;
using Platform.Workflow.Abstractions;

namespace Platform.Workflow.Services;

public class RemoteWorkflowDefinitionService(HttpClient httpClient) : IWorkflowDefinitionService
{
    public async Task<IReadOnlyCollection<string>> ListAsync(CancellationToken cancellationToken = default)
    {
        var response = await httpClient.GetFromJsonAsync<List<string>>("workflow-definitions", cancellationToken);
        return response ?? [];
    }

    public async Task<WorkflowDefinitionMetadata?> GetAsync(string definitionId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(definitionId);

        try
        {
            var response = await httpClient.GetFromJsonAsync<WorkflowDefinitionDto>(
                $"workflow-definitions/{Uri.EscapeDataString(definitionId)}",
                cancellationToken);

            return response == null ? null : new WorkflowDefinitionMetadata(
                response.Id,
                response.Name,
                response.Description,
                response.Version,
                response.IsPublished);
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task<WorkflowDefinitionDocument?> GetDocumentAsync(
        string definitionId,
        int? version = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(definitionId);

        var versionOptions = version is int v ? v.ToString() : "Published";
        var url =
            $"workflow-definitions/by-definition-id/{Uri.EscapeDataString(definitionId)}?versionOptions={Uri.EscapeDataString(versionOptions)}";

        using var response = await httpClient.GetAsync(url, cancellationToken);
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            if (version is not null)
                return null;

            // Fallback Latest khi chưa publish.
            url =
                $"workflow-definitions/by-definition-id/{Uri.EscapeDataString(definitionId)}?versionOptions=Latest";
            using var latest = await httpClient.GetAsync(url, cancellationToken);
            if (latest.StatusCode == System.Net.HttpStatusCode.NotFound)
                return null;
            latest.EnsureSuccessStatusCode();
            return await MapDocumentAsync(latest, cancellationToken);
        }

        response.EnsureSuccessStatusCode();
        return await MapDocumentAsync(response, cancellationToken);
    }

    public async Task<bool> DeleteAsync(string definitionId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(definitionId);

        using var response = await httpClient.DeleteAsync(
            $"workflow-definitions/{Uri.EscapeDataString(definitionId)}",
            cancellationToken);

        return response.IsSuccessStatusCode;
    }

    private static async Task<WorkflowDefinitionDocument?> MapDocumentAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var doc = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
        var root = doc.RootElement;

        var id = ReadString(root, "definitionId") ?? ReadString(root, "id") ?? string.Empty;
        if (string.IsNullOrWhiteSpace(id))
            return null;

        var name = ReadString(root, "name");
        var description = ReadString(root, "description");
        var version = ReadInt(root, "version") ?? 1;
        var isPublished = ReadBool(root, "isPublished") ?? false;
        var isLatest = ReadBool(root, "isLatest") ?? false;

        // Clone JsonElement so it survives after disposing JsonDocument.
        object definition = JsonSerializer.Deserialize<JsonElement>(root.GetRawText());

        return new WorkflowDefinitionDocument(id, name, description, version, isPublished, isLatest, definition);
    }

    private static string? ReadString(JsonElement root, string name) =>
        root.TryGetProperty(name, out var p) && p.ValueKind == JsonValueKind.String ? p.GetString() : null;

    private static int? ReadInt(JsonElement root, string name) =>
        root.TryGetProperty(name, out var p) && p.TryGetInt32(out var v) ? v : null;

    private static bool? ReadBool(JsonElement root, string name) =>
        root.TryGetProperty(name, out var p) && (p.ValueKind is JsonValueKind.True or JsonValueKind.False)
            ? p.GetBoolean()
            : null;

    private sealed record WorkflowDefinitionDto(
        string Id,
        string? Name,
        string? Description,
        int Version,
        bool IsPublished);
}
