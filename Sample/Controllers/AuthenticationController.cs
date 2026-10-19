using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Platform.Authentication;
using Platform.Authentication.Jwt;
using Sample.Authentication;

namespace Sample.Controllers;

/// <summary>
/// Probe AuthN — ApiKey, Basic, Bearer; dùng với <c>Sample.http</c> và Swagger.
/// </summary>
[ApiController]
[Route("api/authentication")]
public class AuthenticationController(
    SampleBearerLoginService bearerLogin,
    IOptionsMonitor<AuthenticationJwtOption> jwtOptions,
    IPasswordPolicyValidator passwordPolicyValidator) : ControllerBase
{
    /// <summary>Trạng thái principal hiện tại (không bắt buộc đăng nhập).</summary>
    [HttpGet("whoami")]
    [AllowAnonymous]
    public IActionResult WhoAmI() => Ok(BuildSnapshot());

    /// <summary>
    /// Đăng nhập Bearer — verify user trong DB (<c>BasicAuthUser</c>) khi profile Bearer <c>Source=Storage</c>, rồi trả access token JWT.
    /// </summary>
    [HttpPost("bearer-token")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(DevBearerTokenResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> IssueBearerToken([FromBody] BearerLoginRequest request, CancellationToken cancellationToken)
    {
        if (jwtOptions.Get(PlatformAuthenticationSchemes.Bearer).Source != AuthenticationCredentialSource.Storage)
            return NotFound(new { message = "Bearer token endpoint requires Authentication:Schemes:Bearer:Profiles:*:Source=Storage." });

        try
        {
            var result = await bearerLogin.LoginAsync(request, cancellationToken);
            return result is null ? Unauthorized() : Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Kiểm tra mật khẩu theo <see cref="IPasswordPolicyValidator"/> (config + <c>TenantPasswordPolicy</c> khi <c>Source=Storage</c>).
    /// </summary>
    [HttpPost("password-policy/validate")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(PasswordPolicyValidationResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> ValidatePasswordPolicy(
        [FromBody] ValidatePasswordPolicyRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(request.Password))
            return BadRequest(new { message = "Password is required." });

        PasswordPolicyScope? scope = request.TenantId is { } tenantId
            ? new PasswordPolicyScope { TenantId = tenantId }
            : null;

        var result = await passwordPolicyValidator.ValidateAsync(request.Password, scope, cancellationToken);
        return Ok(new PasswordPolicyValidationResponse(result.Succeeded, result.Errors));
    }

    /// <summary>Yêu cầu scheme mặc định (<c>Authentication:DefaultAuthenticateScheme</c>, thường <c>ApiKey</c>).</summary>
    [HttpGet("me")]
    [Authorize]
    public IActionResult Me() => Ok(BuildSnapshot());

    [HttpGet("me/api-key")]
    [Authorize(AuthenticationSchemes = PlatformAuthenticationSchemes.ApiKey)]
    public IActionResult MeApiKey() => Ok(BuildSnapshot());

    [HttpGet("me/basic")]
    [Authorize(AuthenticationSchemes = PlatformAuthenticationSchemes.Basic)]
    public IActionResult MeBasic() => Ok(BuildSnapshot());

    [HttpGet("me/bearer")]
    [Authorize(AuthenticationSchemes = PlatformAuthenticationSchemes.Bearer)]
    public IActionResult MeBearer() => Ok(BuildSnapshot());

    private AuthenticationSnapshot BuildSnapshot()
    {
        var identity = User.Identity;
        var claims = User.Claims
            .Select(c => new ClaimValue(c.Type, c.Value))
            .ToList();

        return new AuthenticationSnapshot(
            identity?.IsAuthenticated ?? false,
            identity?.AuthenticationType,
            identity?.Name,
            User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
            claims);
    }

    public sealed record AuthenticationSnapshot(
        bool Authenticated,
        string? Scheme,
        string? Name,
        string? NameIdentifier,
        IReadOnlyList<ClaimValue> Claims);

    public sealed record ClaimValue(string Type, string Value);

    public sealed record ValidatePasswordPolicyRequest(string Password, Guid? TenantId = null);

    public sealed record PasswordPolicyValidationResponse(bool Succeeded, IReadOnlyList<string> Errors);
}
