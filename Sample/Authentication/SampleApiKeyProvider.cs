using System.Security.Claims;
using AspNetCore.Authentication.ApiKey;
using Microsoft.Extensions.Options;
using Jarvis.Authentication.ApiKey;

namespace Sample.Authentication;

/// <summary>
/// Validate API key theo config (kế thừa <see cref="ConfigApiKeyProvider"/>) rồi gắn danh tính demo
/// từ <see cref="SampleDemoIdentityOptions"/>.
/// </summary>
/// <remarks>
/// <para><b>Khi nào dùng:</b> Sample chưa tra user từ DB/vault; các module cần user/tenant
/// (Notifications) sẽ trả 401 nếu principal không có claim id dạng <see cref="Guid"/>.</para>
/// </remarks>
public sealed class SampleApiKeyProvider(
    IOptionsFactory<AuthenticationApiKeyOption> options,
    IOptions<ApiKeyProviderOptions> providerOptions,
    IOptions<SampleDemoIdentityOptions> demoIdentity,
    ILogger<ConfigApiKeyProvider> logger)
    : ConfigApiKeyProvider(options, providerOptions, logger)
{
    public override async Task<IApiKey?> ProvideAsync(string key)
    {
        if (await base.ProvideAsync(key).ConfigureAwait(false) is not { } apiKey)
            return null;

        var identity = demoIdentity.Value;
        if (identity.UserId == Guid.Empty)
            return apiKey;

        var claims = new List<Claim>
        {
            new("tenant_id", identity.TenantId.ToString()),
            new("realm", apiKey.OwnerName ?? string.Empty)
        };

        // OwnerName là nguồn của ClaimTypes.NameIdentifier — ICurrentUser đọc user id từ đây.
        return new ApiKeyModel(apiKey.Key, identity.UserId.ToString(), claims);
    }
}
