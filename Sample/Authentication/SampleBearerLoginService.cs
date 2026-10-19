using Microsoft.Extensions.Options;
using Platform.Authentication;
using Platform.Authentication.Basic;
using Platform.Authentication.Jwt;

namespace Sample.Authentication;

/// <summary>
/// <c>Authentication:Schemes:Bearer:Profiles:*:Source=Storage</c> — verify <see cref="Entities.BasicAuthUser"/> rồi phát JWT.
/// </summary>
public sealed class SampleBearerLoginService(
    IBasicCredentialProvider credentialProvider,
    DevBearerTokenIssuer tokenIssuer,
    IOptionsMonitor<AuthenticationJwtOption> jwtOptions)
{
    public async Task<DevBearerTokenResult?> LoginAsync(BearerLoginRequest request, CancellationToken cancellationToken = default)
    {
        if (jwtOptions.Get(PlatformAuthenticationSchemes.Bearer).Source != AuthenticationCredentialSource.Storage)
            throw new InvalidOperationException("Bearer login is only enabled when Bearer profile Source is Storage.");

        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrEmpty(request.Password))
            return null;

        var validation = await credentialProvider.AuthenticateAsync(
            PlatformAuthenticationSchemes.Basic,
            request.Username.Trim(),
            request.Password,
            cancellationToken);

        return validation is null
            ? null
            : tokenIssuer.IssueFromValidation(validation, request.ExpiresInMinutes);
    }
}

public sealed record BearerLoginRequest(
    string Username,
    string Password,
    int ExpiresInMinutes = 0);
