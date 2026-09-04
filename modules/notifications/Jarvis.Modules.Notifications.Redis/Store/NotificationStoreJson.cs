using System.Text.Json;
using Jarvis.Modules.Notifications.Contracts;

namespace Jarvis.Modules.Notifications.Redis.Store;

/// <summary>
/// JSON item Redis — không có <c>isRead</c> / <c>readAtUtc</c> / <c>expiresAtUtc</c> (ADR D13).
/// </summary>
internal static class NotificationStoreJson
{
    public static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public static string Serialize(SignalRNotificationItemDto item) =>
        JsonSerializer.Serialize(
            new NotificationItemRecord(
                item.NotificationId,
                item.Type,
                item.Title,
                item.Body,
                item.Data,
                item.CreatedAtUtc),
            Options);

    public static SignalRNotificationItemDto? Deserialize(string? json, bool isRead = false)
    {
        if (string.IsNullOrWhiteSpace(json))
            return null;

        var record = JsonSerializer.Deserialize<NotificationItemRecord>(json, Options);
        if (record is null)
            return null;

        return new SignalRNotificationItemDto
        {
            NotificationId = record.NotificationId,
            Type = record.Type,
            Title = record.Title,
            Body = record.Body,
            Data = record.Data,
            CreatedAtUtc = record.CreatedAtUtc,
            IsRead = isRead
        };
    }

    private sealed record NotificationItemRecord(
        Guid NotificationId,
        string Type,
        string Title,
        string? Body,
        JsonElement? Data,
        DateTimeOffset CreatedAtUtc);
}
