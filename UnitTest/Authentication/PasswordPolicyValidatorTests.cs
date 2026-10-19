using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Platform.Authentication;

namespace UnitTest.Authentication;

public class PasswordPolicyValidatorTests
{
    [Fact]
    public async Task AUTH_B_07_Storage_source_uses_provider()
    {
        var services = new ServiceCollection();
        services.Configure<AuthenticationRootOptions>(o =>
        {
            o.PasswordPolicy.Source = PasswordPolicySource.Storage;
            o.PasswordPolicy.Defaults.MinLength = 6;
        });
        services.AddSingleton<IPasswordPolicyScopeProvider, EmptyPasswordPolicyScopeProvider>();
        services.AddSingleton<IPasswordPolicyProvider>(_ => new StubPasswordPolicyProvider(new PasswordPolicyOptions { MinLength = 10 }));
        services.AddSingleton<IPasswordPolicyValidator, PlatformPasswordPolicyValidator>();

        var validator = services.BuildServiceProvider().GetRequiredService<IPasswordPolicyValidator>();

        var fail = await validator.ValidateAsync("123456789");
        Assert.False(fail.Succeeded);
    }

    [Fact]
    public async Task AUTH_B_08_Unset_source_uses_password_policy_class_defaults()
    {
        var services = new ServiceCollection();
        services.Configure<AuthenticationRootOptions>(o => o.PasswordPolicy.Source = null);
        services.AddSingleton<IPasswordPolicyScopeProvider, EmptyPasswordPolicyScopeProvider>();
        services.AddSingleton<IPasswordPolicyProvider, NullPasswordPolicyProvider>();
        services.AddSingleton<IPasswordPolicyValidator, PlatformPasswordPolicyValidator>();

        var validator = services.BuildServiceProvider().GetRequiredService<IPasswordPolicyValidator>();

        Assert.False((await validator.ValidateAsync("short")).Succeeded);
        Assert.True((await validator.ValidateAsync("longenough")).Succeeded);
    }

    [Fact]
    public void AUTH_B_09_Storage_without_provider_fails_options_validation()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IPasswordPolicyProvider, NullPasswordPolicyProvider>();
        var registrationValidator = new PasswordPolicyRegistrationValidator(services.BuildServiceProvider());

        var result = registrationValidator.Validate(
            null,
            new AuthenticationRootOptions { PasswordPolicy = { Source = PasswordPolicySource.Storage } });

        Assert.True(result.Failed);
    }

    [Fact]
    public async Task AUTH_B_10_Storage_provider_returns_null_throws_on_validate()
    {
        var services = new ServiceCollection();
        services.Configure<AuthenticationRootOptions>(o => o.PasswordPolicy.Source = PasswordPolicySource.Storage);
        services.AddSingleton<IPasswordPolicyScopeProvider, EmptyPasswordPolicyScopeProvider>();
        services.AddSingleton<IPasswordPolicyProvider, NullPasswordPolicyProvider>();
        services.AddSingleton<IPasswordPolicyValidator, PlatformPasswordPolicyValidator>();

        var validator = services.BuildServiceProvider().GetRequiredService<IPasswordPolicyValidator>();

        await Assert.ThrowsAsync<InvalidOperationException>(() => validator.ValidateAsync("any-password"));
    }

    private sealed class StubPasswordPolicyProvider(PasswordPolicyOptions policy) : IPasswordPolicyProvider
    {
        public Task<PasswordPolicyOptions?> GetPolicyAsync(PasswordPolicyScope scope, CancellationToken cancellationToken = default) =>
            Task.FromResult<PasswordPolicyOptions?>(policy);
    }
}
