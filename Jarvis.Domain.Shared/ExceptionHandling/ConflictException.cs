using System.Net;

namespace Jarvis.Domain.Shared.ExceptionHandling;

/// <summary>
/// The class representing exceptions conflict. <br />
/// The request will return HttpStatusCode = 409 (Conflict) <br />
/// Example: <br />
/// - Action create but the resource is duplicated <br />
/// - Action update but the new resource is duplicated
/// </summary>
public class ConflictException : BusinessException
{
    /// <summary>
    /// The http status code response is 409
    /// </summary>
    public override HttpStatusCode HttpStatusCode { get; } = HttpStatusCode.Conflict;

    public ConflictException(
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