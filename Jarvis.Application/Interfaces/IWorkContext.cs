using Jarvis.Shared.Auth;

namespace Jarvis.Application.Interfaces;

/// <summary>
/// The unit representing a working session. Inject HttpContext
/// </summary>
public interface IWorkContext
{
    /// <summary>
    /// Return token key from JWT
    /// </summary>
    /// <returns></returns>
    Guid GetTokenKey();

    /// <summary>
    /// Return tenant key from JWT
    /// </summary>
    /// <returns></returns>
    Guid GetTenantKey();

    /// <summary>
    /// Return user key from JWT
    /// </summary>
    /// <returns></returns>
    Guid GetUserKey();

    /// <summary>
    /// Return user name from JWT
    /// </summary>
    /// <returns></returns>
    string GetUserName();

    /// <summary>
    /// Return full name from JWT
    /// </summary>
    /// <returns></returns>
    string GetUserFullName();

    /// <summary>
    /// Return full user info from claim UserInfo
    /// </summary>
    /// <returns></returns>
    UserInfoModel GetUserInfo();
}