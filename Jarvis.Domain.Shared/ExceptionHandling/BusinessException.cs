using System.Net;

namespace Jarvis.Domain.Shared.ExceptionHandling;

/// <summary>
/// The class representing exceptions that have error codes defined by business logic. <br />
/// The request will return HttpStatusCode = 422 (UnprocessableEntity)
/// </summary>
public class BusinessException : Exception
{
    /// <summary>
    /// The error codes are defined by business logic. <br />
    /// Will be set in the field Code of class BaseResponse
    /// </summary>
    public string Code { get; set; }

    /// <summary>
    /// Message of system. Ex: exception system, exception http request. <br />
    /// Will be set in the field Error.SystemMessage of class BaseResponse
    /// </summary>
    public string? SystemMessage { get; set; }

    public object? Content { get; set; }

    /// <summary>
    /// The http status code response is 422
    /// </summary>
    public virtual HttpStatusCode HttpStatusCode { get; } = HttpStatusCode.UnprocessableEntity;

    public BusinessException(
        string code,
        string? systemMessage = null,
        Exception? innerException = null)
        : base(ErrorCodeHelper.GetMessage(code), innerException)
    {
        HResult = int.Parse(code);
        Code = code;
        SystemMessage = systemMessage;
    }

    public BusinessException(
        string code,
        object content,
        string? systemMessage = null,
        Exception? innerException = null)
        : base(ErrorCodeHelper.GetMessage(code), innerException)
    {
        HResult = int.Parse(code);
        Content = content;
        Code = code;
        SystemMessage = systemMessage;
    }
}
