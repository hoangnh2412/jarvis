using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Platform.Authentication;

namespace Platform.Authentication.Basic;

/// <summary>
/// Route Basic auth theo <see cref="AuthenticationBasicOption.Source"/> của profile active.
/// </summary>
public sealed class ProfileSourcedBasicCredentialProvider<TStorage>(
    IOptions<AuthenticationRootOptions> rootOptions,
    IConfiguration configuration,
    IOptionsFactory<AuthenticationBasicOption> optionsFactory,
    ConfigBasicCredentialProvider configurationProvider,
    TStorage storageProvider)
    : IBasicCredentialProvider
    where TStorage : class, IBasicCredentialProvider
{
    public Task<BasicValidationResult?> AuthenticateAsync(
        string schemeName,
        string username,
        string password,
        CancellationToken cancellationToken = default)
    {
        var profile = AuthenticationSchemeProfileConfiguration.ResolveBasicProfile(rootOptions.Value, configuration);
        var source = optionsFactory.Create(profile).Source;
        return source == AuthenticationCredentialSource.Storage
            ? storageProvider.AuthenticateAsync(schemeName, username, password, cancellationToken)
            : configurationProvider.AuthenticateAsync(schemeName, username, password, cancellationToken);
    }
}
