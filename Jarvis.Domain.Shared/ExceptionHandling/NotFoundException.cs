using System.Net;

namespace Jarvis.Domain.Shared.ExceptionHandling;

/// <summary>
/// The class representing exceptions not found the resource. <br />
/// The request will return HttpStatusCode = 404 (NotFound) <br />
/// </summary>
public class NotFoundException : BusinessException
{
    /// <summary>
    /// The http status code response is 404
    /// </summary>
    public override HttpStatusCode HttpStatusCode { get; } = HttpStatusCode.NotFound;

    public NotFoundException(
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