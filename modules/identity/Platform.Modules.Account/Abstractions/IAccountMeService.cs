namespace Platform.Modules.Account.Abstractions;

public sealed record AccountMeOrgUnitDto(
    Guid Id,
    string Code,
    string Name,
    string UnitType,
    string Path);

public sealed record AccountMeUserDto(
    Guid Id,
    string UserName,
    string? DisplayName,
    string? Email);

public sealed record AccountMeResponseDto(
    IReadOnlyList<string> Permissions,
    IReadOnlyList<string> Roles,
    AccountMeOrgUnitDto? OrganizationalUnit,
    IReadOnlyList<AccountMeOrgUnitDto> Descendants,
    AccountMeUserDto Account);

public interface IAccountMeService
{
    Task<AccountMeResponseDto> GetMeAsync(CancellationToken cancellationToken = default);
}
