using Platform.DDD.Domain.Shared.Enums;

namespace Platform.DDD.Domain.Shared.ExceptionHandling;

public class ConcurrentUpdateException : ConflictException
{
    public ConcurrentUpdateException(
        string code = BaseErrorCode.ConcurrentUpdateOccurred,
        string? systemMessage = null,
        Exception? innerException = null)
        : base(code, systemMessage, innerException)
    {
    }
}
