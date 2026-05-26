using System.Net;
using Jarvis.Domain.Shared.Enums;

namespace Jarvis.Domain.Shared.ExceptionHandling;

public class RateLimitedException : BusinessException
{
    /// <summary>
    /// The http status code response is 409
    /// </summary>
    public override HttpStatusCode HttpStatusCode { get; } = HttpStatusCode.TooManyRequests;

    public int Limit { get; }
    public int RetryAfter { get; }

    public RateLimitedException(
        string code = BaseErrorCode.RateLimited,
        int limit = 0,
        int retryAfter = 0,
        string? systemMessage = null,
        Exception? innerException = null)
        : base(code, ErrorCodeHelper.GetMessage(code), systemMessage, innerException)
    {
        HResult = int.Parse(code);
        Code = code;
        SystemMessage = systemMessage;
        Limit = limit;
        RetryAfter = retryAfter;
    }
}
