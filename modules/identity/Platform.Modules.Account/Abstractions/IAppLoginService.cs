namespace Platform.Modules.Account.Abstractions;

public sealed record AppLoginResult(
    string AccessToken,
    string RefreshToken,
    Guid UserId,
    Guid TenantId,
    Guid OrgId);

public interface IAppLoginService
{
    Task<AppLoginResult> LoginAsync(
        string tenantCode,
        string orgCode,
        string userName,
        string password,
        CancellationToken cancellationToken = default);

    Task<AppLoginResult> RefreshAsync(string refreshToken, CancellationToken cancellationToken = default);
}
