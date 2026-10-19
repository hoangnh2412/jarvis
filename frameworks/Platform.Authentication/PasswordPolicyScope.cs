namespace Platform.Authentication;

/// <summary>Phạm vi tra policy — tenant / org / user (host định nghĩa ý nghĩa trong <see cref="IPasswordPolicyProvider"/>).</summary>
public sealed class PasswordPolicyScope
{
    public Guid? TenantId { get; init; }

    public Guid? OrganizationId { get; init; }

    public string? UserId { get; init; }

    public static PasswordPolicyScope Empty => new();
}
