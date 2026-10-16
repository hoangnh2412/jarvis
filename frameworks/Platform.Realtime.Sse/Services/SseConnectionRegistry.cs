using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http;

namespace Platform.Realtime.Sse.Services;

internal sealed record SseFanOutPayload(
    string GroupName,
    string EventName,
    string JsonData);

internal sealed class SseConnectionEntry
{
    private readonly HttpResponse _response;
    private readonly SemaphoreSlim _writeLock = new(1, 1);

    public SseConnectionEntry(string connectionId, IReadOnlyList<string> groups, HttpResponse response)
    {
        ConnectionId = connectionId;
        Groups = groups;
        _response = response;
    }

    public string ConnectionId { get; }
    public IReadOnlyList<string> Groups { get; }

    public async Task WriteSseEventAsync(string eventName, string jsonData, CancellationToken cancellationToken)
    {
        await _writeLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            await _response.WriteAsync($"event: {eventName}\n", cancellationToken).ConfigureAwait(false);
            await _response.WriteAsync($"data: {jsonData}\n\n", cancellationToken).ConfigureAwait(false);
            await _response.Body.FlushAsync(cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            _writeLock.Release();
        }
    }

    public async Task WriteHeartbeatAsync(CancellationToken cancellationToken)
    {
        await _writeLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            var heartbeat = $": heartbeat {DateTime.UtcNow:O}\n\n";
            await _response.WriteAsync(heartbeat, cancellationToken).ConfigureAwait(false);
            await _response.Body.FlushAsync(cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            _writeLock.Release();
        }
    }
}

public sealed class SseConnectionRegistry
{
    private readonly object _sync = new();
    private readonly Dictionary<string, SseConnectionEntry> _connections = new(StringComparer.Ordinal);
    private readonly Dictionary<string, HashSet<string>> _groupIndex = new(StringComparer.Ordinal);

    internal SseConnectionEntry Register(string connectionId, IReadOnlyList<string> groups, HttpResponse response)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionId);
        ArgumentNullException.ThrowIfNull(groups);
        ArgumentNullException.ThrowIfNull(response);

        var entry = new SseConnectionEntry(connectionId, groups, response);
        lock (_sync)
        {
            _connections[connectionId] = entry;
            foreach (var group in groups)
            {
                if (!_groupIndex.TryGetValue(group, out var connectionIds))
                {
                    connectionIds = new HashSet<string>(StringComparer.Ordinal);
                    _groupIndex[group] = connectionIds;
                }

                connectionIds.Add(connectionId);
            }
        }

        return entry;
    }

    public void Unregister(string connectionId)
    {
        lock (_sync)
        {
            if (!_connections.TryGetValue(connectionId, out var entry))
                return;

            _connections.Remove(connectionId);
            foreach (var group in entry.Groups)
            {
                if (_groupIndex.TryGetValue(group, out var connectionIds))
                {
                    connectionIds.Remove(connectionId);
                    if (connectionIds.Count == 0)
                        _groupIndex.Remove(group);
                }
            }
        }
    }

    internal IReadOnlyList<SseConnectionEntry> GetByGroup(string groupName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(groupName);
        lock (_sync)
        {
            if (!_groupIndex.TryGetValue(groupName, out var connectionIds) || connectionIds.Count == 0)
                return Array.Empty<SseConnectionEntry>();

            var entries = new List<SseConnectionEntry>(connectionIds.Count);
            foreach (var connectionId in connectionIds)
            {
                if (_connections.TryGetValue(connectionId, out var entry))
                    entries.Add(entry);
            }

            return entries;
        }
    }

    internal static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };
}
