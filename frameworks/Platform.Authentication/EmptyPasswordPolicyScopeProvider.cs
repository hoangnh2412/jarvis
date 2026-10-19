namespace Platform.Authentication;

/// <summary>Scope rỗng — phù hợp khi <see cref="PasswordPolicySource.Configuration"/>.</summary>
public sealed class EmptyPasswordPolicyScopeProvider : IPasswordPolicyScopeProvider
{
    public Task<PasswordPolicyScope> GetScopeAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(PasswordPolicyScope.Empty);
}
