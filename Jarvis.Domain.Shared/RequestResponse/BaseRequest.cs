using System.Text.Json.Serialization;

namespace Jarvis.Domain.Shared.RequestResponse;

public class BaseRequest
{
    /// <summary>
    /// From header request X-CLIENT-TYPE
    /// </summary>
    [JsonIgnore]
    public virtual string ClientType { get; set; } = string.Empty;

    /// <summary>
    /// From header request X-CLIENT-VERSION. Format: x.y.z
    /// </summary>
    [JsonIgnore]
    public virtual string ClientVersion { get; set; } = string.Empty;

    /// <summary>
    /// From header request X-DEVICE-TYPE. Android, IOS, Browser
    /// </summary>
    [JsonIgnore]
    public virtual string DeviceType { get; set; } = string.Empty;
}