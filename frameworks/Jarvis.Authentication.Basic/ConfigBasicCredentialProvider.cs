using Microsoft.Extensions.Options;

namespace Jarvis.Authentication.Basic;

/// <summary>
/// Provider mặc định — đọc credential từ <see cref="AuthenticationBasicOption.Users"/> trong config.
/// </summary>
public sealed class ConfigBasicCredentialProvider(IOptionsMonitor<AuthenticationBasicOption> options)
    : IBasicCredentialProvider
{
    public Task<BasicValidationResult?> AuthenticateAsync(
        string schemeName,
        string username,
        string password,
        CancellationToken cancellationToken = default)
    {
        var scheme = options.Get(schemeName);
        if (!scheme.Users.TryGetValue(username, out var user))
        {
            return Task.FromResult<BasicValidationResult?>(null);
        }

        return Task.FromResult(BasicValidationResult.Validate(username, password, user));
    }
}
