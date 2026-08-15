namespace Jarvis.Mvc.ExceptionHandling;

public class MiddlewareOption
{
    /// <summary>
    /// The configuration allow the use middleware
    /// </summary>
    public bool IsEnable { get; set; } = false;

    /// <summary>
    /// Middleware applies only when request path matches Includes.
    /// If empty, all paths are considered included.
    /// </summary>
    public string[] Includes { get; set; } = [];

    /// <summary>
    /// Middleware is skipped when request path matches Excludes.
    /// </summary>
    public string[] Excludes { get; set; } = [];
}