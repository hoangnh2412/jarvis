using Jarvis.Shared.Auth;

namespace Jarvis.Application.Interfaces;

/// <summary>
/// The unit representing a working session. Inject HttpContext
/// </summary>
public interface IWorkContext
{
    Guid TokenId { get; }
    Guid TenantId { get; }

    Guid UserId { get; }
    string UserName { get; }
    string FirstName { get; }
    string LastName { get; }
    string FullName { get; }

    /// <summary>
    /// Return full user info from claim UserInfo
    /// </summary>
    /// <returns></returns>
    UserInfoModel UserInfo { get; }

    bool IsInRole(string roleName);
}