using System.Net;

namespace Jarvis.Domain.Shared.ExceptionHandling;

/// <summary>
/// The class representing bad request. <br />
/// The request will return HttpStatusCode = 400 (BadRequest) <br />
/// Example: <br />
/// - Invalid input <br />
/// </summary>
public class BadRequestException : BusinessException
{
    /// <summary>
    /// The http status code response is 400
    /// </summary>
    public override HttpStatusCode HttpStatusCode { get; } = HttpStatusCode.BadRequest;

    public BadRequestException(
        string code,
        string? systemMessage = null,
        Exception? innerException = null)
        : base(code, ErrorCodeHelper.GetMessage(code), systemMessage, innerException)
    {
        HResult = int.Parse(code);
        Code = code;
        SystemMessage = systemMessage;
    }
}