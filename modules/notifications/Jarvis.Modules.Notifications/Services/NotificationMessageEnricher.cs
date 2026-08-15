using System.Text.Json;
using Jarvis.Modules.Notifications.Contracts;

namespace Jarvis.Modules.Notifications.Services;

internal static class NotificationMessageEnricher
{
    public static SignalRNotificationMessage Enrich(
        SignalRNotificationMessage message,
        Guid? userId = null,
        Guid? tenantId = null)
    {
        ArgumentNullException.ThrowIfNull(message);

        return new SignalRNotificationMessage
        {
            NotificationId = message.NotificationId == Guid.Empty
                ? Guid.CreateVersion7()
                : message.NotificationId,
            Type = message.Type,
            Title = message.Title,
            Body = message.Body,
            Data = NormalizeData(message.Data),
            UserId = message.UserId ?? userId,
            TenantId = message.TenantId ?? tenantId,
            CreatedAtUtc = message.CreatedAtUtc == default
                ? DateTimeOffset.UtcNow
                : message.CreatedAtUtc
        };
    }

    private static JsonElement? NormalizeData(JsonElement? data)
    {
        if (data is not { ValueKind: not JsonValueKind.Null and not JsonValueKind.Undefined })
            return null;

        return JsonSerializer.Deserialize<JsonElement>(data.Value.GetRawText());
    }
}
