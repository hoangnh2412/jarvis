using System.Net;

namespace Jarvis.Domain.Shared.ExceptionHandling;

public class ForbiddenException : BusinessException
{
    /// <summary>
    /// The http status code response is 403
    /// </summary>
    public override HttpStatusCode HttpStatusCode { get; } = HttpStatusCode.Forbidden;

    public ForbiddenException(
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