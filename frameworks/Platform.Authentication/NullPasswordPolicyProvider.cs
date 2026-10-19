namespace Platform.Authentication;

/// <summary>Placeholder — host đăng ký <see cref="IPasswordPolicyProvider"/> thật khi <see cref="PasswordPolicySource.Storage"/>.</summary>
public sealed class NullPasswordPolicyProvider : IPasswordPolicyProvider
{
    public Task<PasswordPolicyOptions?> GetPolicyAsync(PasswordPolicyScope scope, CancellationToken cancellationToken = default) =>
        Task.FromResult<PasswordPolicyOptions?>(null);
}
