namespace Platform.Modules.IdentityAdmin.Commands;

using Platform.DDD.Domain.Shared.Messaging;

/// <summary>
/// Assign a policy to a role.
/// Enforces scope check: actor must have the policy being granted.
/// </summary>
public sealed record AssignPolicyToRoleCommand : ICommand
{
    public required Guid RoleId { get; init; }
    public required string PolicyCode { get; init; }
}
