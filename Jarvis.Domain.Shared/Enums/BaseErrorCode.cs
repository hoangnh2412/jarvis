namespace Jarvis.Domain.Shared.Enums;

/// <summary>
/// The class defining the default error codes
/// </summary>
public class BaseErrorCode
{
    /// <summary>
    /// The request return HttpStatusCode = 200
    /// </summary>
    public const string Default200 = "20000000";

    /// <summary>
    /// The request return HttpStatusCode = 400
    /// </summary>
    public const string Default400 = "40000000";

    /// <summary>
    /// The request return HttpStatusCode = 401
    /// </summary>
    public const string Default401 = "40100000";

    /// <summary>
    /// The request return HttpStatusCode = 403
    /// </summary>
    public const string Default403 = "40300000";

    /// <summary>
    /// The request return HttpStatusCode = 404
    /// </summary>
    public const string Default404 = "40400000";

    /// <summary>
    /// The request return HttpStatusCode = 409
    /// </summary>
    public const string Default409 = "40900000";

    /// <summary>
    /// The request return HttpStatusCode = 500
    /// </summary>
    public const string Default500 = "50000000";

    /// <summary>
    /// The is a concurrent update occurred
    /// </summary>
    public const string ConcurrentUpdateOccurred = "40900001";

}