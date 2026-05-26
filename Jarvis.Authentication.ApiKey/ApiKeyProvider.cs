using AspNetCore.Authentication.ApiKey;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Jarvis.Authentication.ApiKey;

/// <summary>
/// Provider mặc định validate API key từ header, đọc cấu hình <see cref="AuthenticationApiKeyOption"/> theo realm/scheme.
/// </summary>
/// <remarks>
/// <para>
/// Được đăng ký tự động qua <c>AddCoreApiKey</c>. Middleware AspNetCore.Authentication.ApiKey
/// gọi <see cref="ProvideAsync"/> với giá trị header thô (ví dụ <c>my-secret</c> hoặc <c>Default:my-secret</c>).
/// </para>
/// <para><b>Luồng xử lý:</b></para>
/// <list type="number">
/// <item>Có dạng <c>{realm}:{secret}</c> → tra config theo <c>realm</c>, so khớp theo <see cref="ApiKeyMode"/> của realm đó.</item>
/// <item>Không có <c>:</c> → coi là <see cref="ApiKeyMode.SingleKey"/> trên scheme mặc định (<see cref="ApiKeyProviderOptions.DefaultSchemeName"/>).</item>
/// </list>
/// <para>
/// <b>Khi nào dùng:</b> host chỉ cần danh sách key tĩnh trong <c>appsettings</c> (<c>Authentication:ApiKey:{realm}:Keys</c>).
/// Cần validate động (DB, vault, cache) → kế thừa class này hoặc implement <see cref="IApiKeyProvider"/> riêng
/// và đăng ký qua <c>AddCoreApiKey&lt;T&gt;</c>.
/// </para>
/// </remarks>
public class ApiKeyProvider(
    IOptionsFactory<AuthenticationApiKeyOption> options,
    IOptions<ApiKeyProviderOptions> providerOptions,
    ILogger<ApiKeyProvider> logger) : IApiKeyProvider
{
    private readonly IOptionsFactory<AuthenticationApiKeyOption> _options = options;
    private readonly IOptions<ApiKeyProviderOptions> _providerOptions = providerOptions;
    private readonly ILogger<ApiKeyProvider> _logger = logger;

    /// <summary>
    /// Xác thực giá trị API key từ request header.
    /// </summary>
    /// <param name="key">Giá trị header thô (secret thuần hoặc <c>realm:secret</c>).</param>
    /// <returns><see cref="ApiKeyModel"/> nếu hợp lệ; <c>null</c> nếu rỗng, sai format, hoặc không khớp <c>Keys[]</c>.</returns>
    public virtual Task<IApiKey?> ProvideAsync(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            return Task.FromResult<IApiKey?>(null);

        if (TryParseRealmKey(key, out var realm, out var secret))
            return Task.FromResult<IApiKey?>(ValidateRealmKey(realm, secret, key));

        return Task.FromResult<IApiKey?>(ValidateSingleKey(key));
    }

    /// <summary>
    /// Validate key không có prefix realm — dùng scheme mặc định và mode <see cref="ApiKeyMode.SingleKey"/>.
    /// </summary>
    private ApiKeyModel? ValidateSingleKey(string key)
    {
        var schemeName = _providerOptions.Value.DefaultSchemeName;
        var option = _options.Create(schemeName);

        if (option.Mode == ApiKeyMode.RealmKey)
        {
            _logger.LogDebug("API key rejected: scheme {Scheme} expects RealmKey format.", schemeName);
            return null;
        }

        if (!option.KeySet.Contains(key))
            return null;

        return new ApiKeyModel(key, schemeName, null);
    }

    /// <summary>
    /// Validate key dạng <c>realm:secret</c> — tra named options theo <paramref name="realm"/>.
    /// </summary>
    /// <remarks>
    /// Realm <see cref="ApiKeyMode.SingleKey"/>: so khớp toàn bộ <paramref name="rawKey"/>.
    /// Realm <see cref="ApiKeyMode.RealmKey"/>: chỉ so khớp phần <paramref name="secret"/> sau dấu <c>:</c>.
    /// </remarks>
    private ApiKeyModel? ValidateRealmKey(string realm, string secret, string rawKey)
    {
        var option = _options.Create(realm);
        if (option.Keys.Length == 0 && string.IsNullOrEmpty(option.KeyName))
        {
            _logger.LogDebug("API key realm {Realm} is not configured.", realm);
            return null;
        }

        if (option.Mode == ApiKeyMode.SingleKey)
        {
            if (!option.KeySet.Contains(rawKey))
                return null;

            return new ApiKeyModel(rawKey, realm, null);
        }

        if (!option.KeySet.Contains(secret))
            return null;

        return new ApiKeyModel(rawKey, realm, null);
    }

    /// <summary>
    /// Tách <c>realm:secret</c> tại dấu <c>:</c> đầu tiên.
    /// </summary>
    /// <returns><c>false</c> nếu không có <c>:</c>, hoặc <c>:</c> ở đầu/cuối chuỗi.</returns>
    private static bool TryParseRealmKey(string key, out string realm, out string secret)
    {
        var index = key.IndexOf(':');
        if (index <= 0 || index >= key.Length - 1)
        {
            realm = string.Empty;
            secret = string.Empty;
            return false;
        }

        realm = key[..index];
        secret = key[(index + 1)..];
        return true;
    }
}
