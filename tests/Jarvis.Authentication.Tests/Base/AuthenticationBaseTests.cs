using Jarvis.Authentication.Tests.Helpers;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Jarvis.Authentication.Tests.Base;

/// <summary>Test <see cref="AddJarvisAuthentication"/> — bind root options và default scheme.</summary>
public class AuthenticationBaseTests
{
    [Fact]
    public void AUTH_B_01_Valid_config_binds_root_options()
    {
        var config = AuthenticationConfigurationBuilder.BuildRootConfig(new Dictionary<string, string?>
        {
            ["Authentication:Type"] = "ApiKey",
            ["Authentication:DefaultAuthenticateScheme"] = "Default"
        });

        using var host = Host.CreateDefaultBuilder()
            .ConfigureServices(services => services.AddJarvisAuthentication(config))
            .Build();

        var options = host.Services.GetRequiredService<IOptions<AuthenticationRootOptions>>().Value;
        Assert.Equal("ApiKey", options.Type);
        Assert.Equal("Default", options.DefaultAuthenticateScheme);
    }

    [Fact]
    public void AUTH_B_02_Empty_type_fails_validation()
    {
        var validator = new AuthenticationRootOptionsValidator();
        var result = validator.Validate(null, new AuthenticationRootOptions { Type = "" });

        Assert.True(result.Failed);
        Assert.Contains("Authentication:Type", result.FailureMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void AUTH_B_03_Default_authenticate_scheme_bearer()
    {
        var config = AuthenticationConfigurationBuilder.BuildRootConfig(new Dictionary<string, string?>
        {
            ["Authentication:Type"] = "Jwt",
            ["Authentication:DefaultAuthenticateScheme"] = "Bearer",
            ["Authentication:DefaultChallengeScheme"] = "Bearer"
        });

        var services = new ServiceCollection();
        services.AddJarvisAuthentication(config);
        var sp = services.BuildServiceProvider();

        var options = sp.GetRequiredService<IOptions<AuthenticationOptions>>().Value;
        Assert.Equal("Bearer", options.DefaultAuthenticateScheme);
    }

    [Fact]
    public async Task AUTH_B_06_Password_policy_min_length()
    {
        var config = AuthenticationConfigurationBuilder.BuildRootConfig(new Dictionary<string, string?>
        {
            ["Authentication:Type"] = "ApiKey",
            ["Authentication:PasswordPolicy:MinLength"] = "12"
        });

        var services = new ServiceCollection();
        services.Configure<AuthenticationRootOptions>(config.GetSection("Authentication"));
        services.AddSingleton<IPasswordPolicyValidator, DefaultPasswordPolicyValidator>();
        var validator = services.BuildServiceProvider().GetRequiredService<IPasswordPolicyValidator>();

        var shortResult = await validator.ValidateAsync("short");
        var okResult = await validator.ValidateAsync("longenoughpass");

        Assert.False(shortResult.Succeeded);
        Assert.True(okResult.Succeeded);
    }
}
