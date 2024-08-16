namespace Jarvis.Domain.Shared.RequestResponse;

/// <summary>
/// The class define base error of response. Use for http response 400
/// </summary>
public class BaseResponseError
{
    /// <summary>
    /// Message of ErrorCode
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Message of system. Ex: exception system, exception http request
    /// </summary>
    public string SystemMessage { get; set; } = string.Empty;

    /// <summary>
    /// Error detail in ModelState
    /// </summary>
    public Dictionary<string, IList<string>> Details { get; set; } = new Dictionary<string, IList<string>>();
}