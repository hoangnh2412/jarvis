using System.Net;

namespace Jarvis.Domain.Shared.ExceptionHandling;

/// <summary>
/// The class representing exceptions but not defined. <br />
/// The request will return HttpStatusCode = 500 (Internal Server Error) <br />
/// </summary>
public class InternalErrorException : BusinessException
{
    /// <summary>
    /// The http status code response is 500
    /// </summary>
    public override HttpStatusCode HttpStatusCode { get; } = HttpStatusCode.InternalServerError;

    public InternalErrorException(
        string code,
        string? message = null,
        string? systemMessage = null,
        Exception? innerException = null)
        : base(code, message, systemMessage, innerException)
    {
        HResult = int.Parse(code);
        Code = code;
        SystemMessage = systemMessage;
    }
}