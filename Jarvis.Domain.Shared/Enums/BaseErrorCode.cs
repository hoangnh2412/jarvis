namespace Jarvis.Domain.Shared.Enums;

/// <summary>
/// The class defining the default error codes
/// </summary>
public class BaseErrorCode
{
    public const string Default = "00000";

    /// <summary>
    /// The is a concurrent update occurred
    /// </summary>
    public const string ConcurrentUpdateOccurred = "40900001";

}