using System.Text.Json;
using Microsoft.Extensions.Logging;
using Platform.Caching.Redis;
using Platform.Workflow.Configuration;

namespace Platform.Workflow.Services;

/// <summary>
/// Thông tin của một workflow node (activity)
/// </summary>
public sealed record WorkflowNodeInfo(
    string ActivityId,
    string? Handler = null,
    DateTime? ProcessedAt = null,
    string? Status = null,
    string? Assignee = null,
    string? Notes = null,
    TimeSpan? Duration = null,
    string? Result = null,
    Dictionary<string, object>? CustomData = null);

public interface IWorkflowNodeInfoTracker
{
    Task RecordNodeInfoAsync(string workflowInstanceId, string activityId, WorkflowNodeInfo info, CancellationToken cancellationToken = default);
    Task<WorkflowNodeInfo?> GetNodeInfoAsync(string workflowInstanceId, string activityId, CancellationToken cancellationToken = default);
    Task<IEnumerable<WorkflowNodeInfo>> GetAllNodeInfoAsync(string workflowInstanceId, CancellationToken cancellationToken = default);
    Task ClearInstanceInfoAsync(string workflowInstanceId, CancellationToken cancellationToken = default);
}

public sealed class PlatformWorkflowNodeInfoTracker : IWorkflowNodeInfoTracker
{
    private readonly IPlatformRedisStore _redisStore;
    private readonly WorkflowStoreOptions _options;
    private readonly ILogger<PlatformWorkflowNodeInfoTracker> _logger;

    public PlatformWorkflowNodeInfoTracker(IPlatformRedisStore redisStore, WorkflowStoreOptions options, ILogger<PlatformWorkflowNodeInfoTracker> logger)
    {
        ArgumentNullException.ThrowIfNull(redisStore);
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(logger);
        _redisStore = redisStore;
        _options = options;
        _logger = logger;
    }

    public async Task RecordNodeInfoAsync(string workflowInstanceId, string activityId, WorkflowNodeInfo info, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(workflowInstanceId);
        ArgumentException.ThrowIfNullOrWhiteSpace(activityId);
        ArgumentNullException.ThrowIfNull(info);
        try
        {
            await _redisStore.SetStringAsync(BuildNodeKey(workflowInstanceId, activityId), JsonSerializer.Serialize(info), _options.NodeInfoTtl, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to record node info for {WorkflowInstanceId}/{ActivityId}", workflowInstanceId, activityId);
        }
    }

    public async Task<WorkflowNodeInfo?> GetNodeInfoAsync(string workflowInstanceId, string activityId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(workflowInstanceId);
        ArgumentException.ThrowIfNullOrWhiteSpace(activityId);
        try
        {
            var json = await _redisStore.GetStringAsync(BuildNodeKey(workflowInstanceId, activityId), cancellationToken);
            return json == null ? null : JsonSerializer.Deserialize<WorkflowNodeInfo>(json);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get node info for {WorkflowInstanceId}/{ActivityId}", workflowInstanceId, activityId);
            return null;
        }
    }

    public async Task<IEnumerable<WorkflowNodeInfo>> GetAllNodeInfoAsync(string workflowInstanceId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(workflowInstanceId);
        try
        {
            var keys = await _redisStore.FindKeysAsync(BuildNodeKeyPattern(workflowInstanceId), cancellationToken);
            var result = new List<WorkflowNodeInfo>();
            foreach (var key in keys)
            {
                var json = await _redisStore.GetStringAsync(key, cancellationToken);
                var info = json == null ? null : JsonSerializer.Deserialize<WorkflowNodeInfo>(json);
                if (info != null) result.Add(info);
            }
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get all node info for {WorkflowInstanceId}", workflowInstanceId);
            return Enumerable.Empty<WorkflowNodeInfo>();
        }
    }

    public async Task ClearInstanceInfoAsync(string workflowInstanceId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(workflowInstanceId);
        try
        {
            var keys = await _redisStore.FindKeysAsync(BuildNodeKeyPattern(workflowInstanceId), cancellationToken);
            foreach (var key in keys)
                await _redisStore.DeleteAsync(key, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to clear node info for {WorkflowInstanceId}", workflowInstanceId);
        }
    }

    private string BuildNodeKey(string workflowInstanceId, string activityId) => $"{_options.KeyPrefix}:node:{workflowInstanceId}:{activityId}";
    private string BuildNodeKeyPattern(string workflowInstanceId) => $"{_options.KeyPrefix}:node:{workflowInstanceId}:*";
}
