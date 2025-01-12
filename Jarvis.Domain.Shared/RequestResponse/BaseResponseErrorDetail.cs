namespace Jarvis.Domain.Shared.RequestResponse;

/// <summary>
/// The class define base error detail of response
/// </summary>
public class BaseResponseErrorDetail
{
    /// <summary>
    /// Field has error
    /// </summary>
    public required string Field { get; set; }

    /// <summary>
    /// List error code of field
    /// </summary>
    public IList<string>? Codes { get; set; }

    /// <summary>
    /// List system message
    /// </summary>
    public IList<string>? SystemMessages { get; set; }
}