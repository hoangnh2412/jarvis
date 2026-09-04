using Jarvis.Modules.Notifications.Contracts;
using Jarvis.Modules.Notifications.Redis.Store;

namespace UnitTest.Notifications;

public class NotificationStoreJsonTests
{
    [Fact]
    public void Serialize_Does_Not_Include_IsRead_ReadAtUtc_ExpiresAtUtc()
    {
        var dto = new SignalRNotificationItemDto
        {
            NotificationId = Guid.NewGuid(),
            Type = "demo",
            Title = "Hi",
            IsRead = true,
            CreatedAtUtc = DateTimeOffset.UtcNow,
        };

        var json = NotificationStoreJson.Serialize(dto);

        Assert.DoesNotContain("isRead", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("readAtUtc", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("expiresAtUtc", json, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("\"title\"", json, StringComparison.OrdinalIgnoreCase);
    }
}
