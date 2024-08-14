using Jarvis.Domain.Shared.Enums;

namespace Jarvis.Domain.Shared.ExceptionHandling;

public class ConcurrentUpdateException(
    string code = BaseErrorCode.ConcurrentUpdateOccurred,
    string? message = null,
    string? systemMessage = null,
    Exception? innerException = null)
    : ConflictException(code, message, systemMessage, innerException)
{
}
