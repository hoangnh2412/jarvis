using Microsoft.Extensions.Options;

namespace Jarvis.Authentication.Jwt;

public sealed class AuthenticationJwtOptionValidator : IValidateOptions<AuthenticationJwtOption>
{
    public ValidateOptionsResult Validate(string? name, AuthenticationJwtOption options)
    {
        var hasAuthority = !string.IsNullOrWhiteSpace(options.Authority);
        var hasSymmetricKeys = options.IssuerSigningKeys.Length > 0;

        if (!hasAuthority && options.ValidateIssuerSigningKey && !hasSymmetricKeys)
        {
            return ValidateOptionsResult.Fail(
                $"Authentication:Jwt:{name ?? "scheme"} requires Authority or IssuerSigningKeys when ValidateIssuerSigningKey is enabled.");
        }

        if (hasAuthority && options.ValidateAudience
            && string.IsNullOrWhiteSpace(options.Audience)
            && options.ValidAudiences.Length == 0)
        {
            return ValidateOptionsResult.Fail(
                $"Authentication:Jwt:{name ?? "scheme"} requires Audience or ValidAudiences when ValidateAudience is enabled.");
        }

        return ValidateOptionsResult.Success;
    }
}
