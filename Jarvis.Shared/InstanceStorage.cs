namespace Jarvis.Shared;

/// <summary>
/// Storage application launch setting parameters
/// </summary>
public static partial class InstanceStorage
{
    public static Dictionary<string, IDictionary<string, string>> Services = new Dictionary<string, IDictionary<string, string>>();
}