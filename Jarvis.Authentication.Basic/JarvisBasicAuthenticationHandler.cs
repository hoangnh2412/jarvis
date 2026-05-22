using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Jarvis.Authentication.Basic;

public sealed class JarvisBasicAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private readonly IBasicCredentialValidator _validator;
    private readonly IOptionsMonitor<AuthenticationBasicOption> _basicOptions;

    public JarvisBasicAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        IBasicCredentialValidator validator,
        IOptionsMonitor<AuthenticationBasicOption> basicOptions)
        : base(options, logger, encoder)
    {
        _validator = validator;
        _basicOptions = basicOptions;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue("Authorization", out var headerValues))
            return AuthenticateResult.NoResult();

        var header = headerValues.ToString();
        if (!header.StartsWith("Basic ", StringComparison.OrdinalIgnoreCase))
            return AuthenticateResult.NoResult();

        string userPass;
        try
        {
            var encoded = header["Basic ".Length..].Trim();
            userPass = Encoding.UTF8.GetString(Convert.FromBase64String(encoded));
        }
        catch (FormatException)
        {
            return AuthenticateResult.Fail("Invalid Basic authentication header.");
        }

        var separatorIndex = userPass.IndexOf(':');
        if (separatorIndex <= 0 || separatorIndex >= userPass.Length - 1)
            return AuthenticateResult.Fail("Invalid Basic credentials format.");

        var username = userPass[..separatorIndex];
        var password = userPass[(separatorIndex + 1)..];

        var validation = await _validator.ValidateAsync(Scheme.Name, username, password, Context.RequestAborted);
        if (validation is null)
            return AuthenticateResult.Fail("Invalid username or password.");

        var identity = new ClaimsIdentity(validation.Claims, Scheme.Name, ClaimTypes.Name, ClaimTypes.Role);
        var principal = new ClaimsPrincipal(identity);
        return AuthenticateResult.Success(new AuthenticationTicket(principal, Scheme.Name));
    }

    protected override Task HandleChallengeAsync(AuthenticationProperties properties)
    {
        var realm = _basicOptions.Get(Scheme.Name).Realm;
        Response.Headers.WWWAuthenticate = $"Basic realm=\"{realm}\"";
        return base.HandleChallengeAsync(properties);
    }
}
