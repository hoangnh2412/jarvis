namespace Jarvis.Domain.Shared.RequestResponse;

/// <summary>
/// The class define base error of response. Use for http response 400
/// </summary>
public class BaseResponseError(string? message = null, string? systemMessage = null)
{
    /// <summary>
    /// Message of ErrorCode
    /// </summary>
    public string? Message { get; set; } = message;

    /// <summary>
    /// Message of system. Ex: exception system, exception http request
    /// </summary>
    public string? SystemMessage { get; set; } = systemMessage;

    /// <summary>
    /// Error detail in ModelState
    /// </summary>
    public IList<BaseResponseErrorDetail>? Details { get; set; }
}