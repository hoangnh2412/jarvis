using Newtonsoft.Json;

namespace Jarvis.Domain.Shared.Utility;

public static class JsonHelper
{
    public static JsonSerializerSettings JsonOption = new JsonSerializerSettings();

    public static string SerializeObject<T>(T value)
    {
        if (value == null)
            throw new NullReferenceException($"{nameof(value)} cannot be null");

        return JsonConvert.SerializeObject(value, JsonOption);
    }

    public static T? DeserializeObject<T>(string value)
    {
        if (value == null)
            throw new NullReferenceException($"{nameof(value)} cannot be null");

        return JsonConvert.DeserializeObject<T>(value, JsonOption);
    }
}