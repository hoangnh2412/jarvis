namespace Jarvis.Domain.Services;

/// <summary>
/// The unit representing a working session. Inject HttpContext
/// </summary>
public interface IWorkContext
{
    /// <summary>
    /// Return token key from JWT
    /// </summary>
    /// <returns></returns>
    Guid? GetTokenId();

    Guid? GetTenantId();

    Guid? GetUserId();

    string? GetUserName();
}