using SystemTextJson = System.Text.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Jarvis.Modules.Notifications.Serialization;

/// <summary>
/// Ghi JSON nhúng từ <see cref="SystemTextJson.JsonElement"/> thay vì serialize metadata struct (vd. valueKind).
/// Cần cho REST khi host dùng Newtonsoft.Json.
/// </summary>
public sealed class JsonElementNewtonsoftConverter : JsonConverter<SystemTextJson.JsonElement?>
{
    public override SystemTextJson.JsonElement? ReadJson(
        JsonReader reader,
        Type objectType,
        SystemTextJson.JsonElement? existingValue,
        bool hasExistingValue,
        JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.Null)
            return null;

        var token = JToken.ReadFrom(reader);
        return SystemTextJson.JsonSerializer.Deserialize<SystemTextJson.JsonElement>(token.ToString());
    }

    public override void WriteJson(
        JsonWriter writer,
        SystemTextJson.JsonElement? value,
        JsonSerializer serializer)
    {
        if (value is not { ValueKind: not SystemTextJson.JsonValueKind.Null and not SystemTextJson.JsonValueKind.Undefined })
        {
            writer.WriteNull();
            return;
        }

        writer.WriteRawValue(value.Value.GetRawText());
    }
}
