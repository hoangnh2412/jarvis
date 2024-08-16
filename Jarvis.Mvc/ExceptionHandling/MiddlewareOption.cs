namespace Jarvis.Mvc.ExceptionHandling;

public class MiddlewareOption
{
    /// <summary>
    /// The configuration allow the use middleware
    /// </summary>
    public bool IsEnable { get; set; } = false;

    /// <summary>
    /// The configuration middleware does not apply the follow rules:
    /// - PathStartWith: Route start with the paths in the list
    /// - Path: Route is in the list
    /// </summary>
    public Dictionary<string, string[]> Ignores { get; set; } = new Dictionary<string, string[]>();
}