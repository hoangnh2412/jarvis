namespace Platform.Modules.IdentityAdmin.Abstractions;

public sealed record IdentityInstallRequest(
    string UserName,
    string Password,
    string? DisplayName);

public sealed record IdentityInstallOutcome(
    IdentityInstallStatus Status,
    Guid? TenantId,
    Guid? UserId,
    IReadOnlyList<string> Errors)
{
    public static IdentityInstallOutcome Created(Guid userId) =>
        new(IdentityInstallStatus.Created, null, userId, []);

    public static IdentityInstallOutcome AlreadyInstalled() =>
        new(IdentityInstallStatus.AlreadyInstalled, null, null, []);

    public static IdentityInstallOutcome InvalidPassword(IReadOnlyList<string> errors) =>
        new(IdentityInstallStatus.InvalidPassword, null, null, errors);
}

public enum IdentityInstallStatus
{
    Created,
    AlreadyInstalled,
    InvalidPassword,
}

public interface ISystemInstallService
{
    Task<IdentityInstallOutcome> InstallAsync(
        IdentityInstallRequest request,
        CancellationToken cancellationToken = default);
}
