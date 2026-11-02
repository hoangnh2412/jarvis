namespace Platform.Modules.IdentityAdmin.Abstractions;

public enum UserOrgScopeUpdateStatus
{
    Updated,
    ScopeMissing,
    NotFound,
}

public sealed record UserOrgScopeResultDto(
    Guid Id,
    Guid? OrganizationalUnitId,
    string? UnitType,
    string? LegalEntityId,
    string? Path);

public sealed record UserOrgScopeUpdateOutcome(
    UserOrgScopeUpdateStatus Status,
    UserOrgScopeResultDto? Data = null);

public interface IUserOrgScopeAdminService
{
    Task<UserOrgScopeUpdateOutcome> SetUserScopeAsync(
        Guid userId,
        Guid? organizationalUnitId,
        CancellationToken cancellationToken = default);
}
