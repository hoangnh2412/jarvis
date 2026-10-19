namespace Platform.Authentication;

/// <summary>Resolve <see cref="PasswordPolicyScope"/> cho request hiện tại (HTTP, job, …).</summary>
public interface IPasswordPolicyScopeProvider
{
    Task<PasswordPolicyScope> GetScopeAsync(CancellationToken cancellationToken = default);
}
