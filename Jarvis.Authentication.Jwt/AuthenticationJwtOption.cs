namespace Jarvis.Authentication.Jwt;

/// <summary>
/// Options cấu hình JWT Bearer — bind từ <c>Authentication:Jwt:{scheme}</c>.
/// </summary>
/// <remarks>
/// <para><b>Chức năng:</b> khai báo authority OIDC, audience, signing keys và các flag validate token.</para>
/// <para><b>Khi nào dùng:</b></para>
/// <list type="bullet">
/// <item>Có <see cref="Authority"/> — validate token qua metadata issuer (OpenIddict, Cognito, Azure AD).</item>
/// <item>Không có Authority — dùng <see cref="IssuerSigningKeys"/> symmetric cho dev/test.</item>
/// <item><see cref="MaxExpireMinutes"/> &gt; 0 — giới hạn lifetime token tùy policy Jarvis.</item>
/// </list>
/// </remarks>
public class AuthenticationJwtOption
{
    /// <summary>URL issuer OIDC (ví dụ <c>https://auth.example.com/</c>). Khi set, bỏ qua symmetric keys.</summary>
    public string? Authority { get; set; }

    /// <summary>Audience mặc định khi dùng Authority.</summary>
    public string? Audience { get; set; }

    /// <summary>Bắt buộc HTTPS cho metadata discovery. Mặc định <c>true</c> khi không set trong config.</summary>
    public bool? RequireHttpsMetadata { get; set; }

    /// <summary>Symmetric signing keys (dev/test) — dùng khi không có <see cref="Authority"/>.</summary>
    public string[] IssuerSigningKeys { get; set; } = [];

    public bool ValidateActor { get; set; }

    public bool ValidateSignatureLast { get; set; } = true;

    public bool ValidateWithLKG { get; set; }

    public bool ValidateTokenReplay { get; set; }

    public bool ValidateAudience { get; set; } = true;

    public string[] ValidAudiences { get; set; } = [];

    public bool ValidateIssuerSigningKey { get; set; } = true;

    public bool ValidateIssuer { get; set; }

    public string[] ValidIssuers { get; set; } = [];

    /// <summary>Giới hạn lifetime token (phút). <c>0</c> = chỉ dùng validate lifetime mặc định của framework.</summary>
    public int MaxExpireMinutes { get; set; }
}
