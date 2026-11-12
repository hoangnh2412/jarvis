namespace Platform.Modules.Account.Abstractions;

public sealed record AppLoginResult(
    string AccessToken,
    string RefreshToken,
    Guid UserId,
    Guid TenantId,
    Guid OrgId);
