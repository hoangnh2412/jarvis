using Microsoft.Extensions.Options;

namespace Jarvis.Authentication.Basic;

/// <summary>
/// Validator mặc định — so khớp username/password với <see cref="AuthenticationBasicOption.Users"/> trong config.
/// </summary>
/// <remarks>
/// <para><b>Chức năng:</b> tra user theo scheme, so password, trả claims Name + Roles.</para>
/// <para><b>Khi nào dùng:</b> đăng ký tự động qua <c>AddCoreBasic(configuration)</c> không generic.
/// Cần nguồn credential khác → <c>AddCoreBasic(configuration, lookup)</c> hoặc <c>AddCoreBasic&lt;T&gt;</c>.</para>
/// </remarks>
public sealed class ConfigBasicCredentialValidator(IOptionsMonitor<AuthenticationBasicOption> options) : IBasicCredentialValidator
{
    public Task<BasicValidationResult?> ValidateAsync(
        string schemeName,
        string username,
        string password,
        CancellationToken cancellationToken = default)
    {
        var scheme = options.Get(schemeName);
        if (!scheme.Users.TryGetValue(username, out var user))
            return Task.FromResult<BasicValidationResult?>(null);

        return Task.FromResult(BasicCredentialValidation.Validate(username, password, user));
    }
}
