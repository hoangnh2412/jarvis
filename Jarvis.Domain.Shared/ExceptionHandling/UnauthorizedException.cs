using System.Net;

namespace Jarvis.Domain.Shared.ExceptionHandling;

public class UnauthorizedException : BusinessException
{
    /// <summary>
    /// The http status code response is 401
    /// </summary>
    public override HttpStatusCode HttpStatusCode { get; } = HttpStatusCode.Unauthorized;

    public UnauthorizedException(
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
