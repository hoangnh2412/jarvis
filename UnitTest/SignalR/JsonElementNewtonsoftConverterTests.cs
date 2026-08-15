using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Jarvis.Modules.Notifications.Contracts;
using Jarvis.Modules.Notifications.Serialization;

namespace UnitTest.SignalR;

public class JsonElementNewtonsoftConverterTests
{
    private static readonly JsonSerializerSettings SerializerSettings = new()
    {
        ContractResolver = new CamelCasePropertyNamesContractResolver(),
        Converters = { new JsonElementNewtonsoftConverter() },
    };

    [Fact]
    public void SignalRNotificationItemDto_Serializes_Data_As_Embedded_Json()
    {
        var dto = new SignalRNotificationItemDto
        {
            NotificationId = Guid.Parse("019fca8e-2cfa-70d6-a283-2031feb900d9"),
            Type = "comment",
            Title = "Test open link",
            Body = "Hover row",
            Data = System.Text.Json.JsonSerializer.SerializeToElement(new
            {
                actionUrl = "/documents",
                actorName = "Demo User",
            }),
            IsRead = false,
            CreatedAtUtc = DateTimeOffset.Parse("2026-08-04T09:15:43.0983095+07:00"),
        };

        var json = JsonConvert.SerializeObject(dto, SerializerSettings);

        Assert.Contains("\"actionUrl\":\"/documents\"", json);
        Assert.Contains("\"actorName\":\"Demo User\"", json);
        Assert.DoesNotContain("valueKind", json, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void SignalRNotificationItemDto_Serializes_Null_Data_As_Null()
    {
        var dto = new SignalRNotificationItemDto
        {
            NotificationId = Guid.NewGuid(),
            Type = "demo",
            Title = "No data",
            Data = null,
            CreatedAtUtc = DateTimeOffset.UtcNow,
        };

        var json = JsonConvert.SerializeObject(dto, SerializerSettings);

        Assert.Contains("\"data\":null", json.Replace(" ", string.Empty));
    }

    [Fact]
    public void SignalRNotificationItemDto_Deserializes_Data_From_Embedded_Json()
    {
        const string json = """
            {
              "notificationId": "019fca8e-2cfa-70d6-a283-2031feb900d9",
              "type": "comment",
              "title": "Test",
              "data": { "actionUrl": "https://example.com" },
              "isRead": false,
              "createdAtUtc": "2026-08-04T02:15:43.0983095Z"
            }
            """;

        var dto = JsonConvert.DeserializeObject<SignalRNotificationItemDto>(json, SerializerSettings);

        Assert.NotNull(dto);
        Assert.NotNull(dto!.Data);
        Assert.Equal("https://example.com", dto.Data!.Value.GetProperty("actionUrl").GetString());
    }
}
