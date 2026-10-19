using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Platform.Authentication;

namespace Platform.Authentication.Basic;

/// <summary>
/// Provider mặc định — đọc credential từ <see cref="AuthenticationBasicOption.Users"/> của profile active.
/// </summary>
public sealed class ConfigBasicCredentialProvider(
    IOptions<AuthenticationRootOptions> rootOptions,
    IConfiguration configuration,
    IOptionsFactory<AuthenticationBasicOption> optionsFactory)
    : IBasicCredentialProvider
{
    public Task<BasicValidationResult?> AuthenticateAsync(
        string schemeName,
        string username,
        string password,
        CancellationToken cancellationToken = default)
    {
        var profile = AuthenticationSchemeProfileConfiguration.ResolveBasicProfile(rootOptions.Value, configuration);
        var scheme = optionsFactory.Create(profile);
        if (!scheme.Users.TryGetValue(username, out var user))
            return Task.FromResult<BasicValidationResult?>(null);

        return Task.FromResult(BasicValidationResult.Validate(username, password, user));
    }
}
