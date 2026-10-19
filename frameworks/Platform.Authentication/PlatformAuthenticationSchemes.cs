namespace Platform.Authentication;

/// <summary>
/// Tên scheme authentication chuẩn — thống nhất config, <c>AddCore*</c> và <c>[Authorize]</c>.
/// </summary>
/// <remarks>
/// <para>Scheme ASP.NET Core: <see cref="Bearer"/>, <see cref="Basic"/>, <see cref="ApiKey"/>.</para>
/// <para>Profile mặc định trong config: <see cref="DefaultProfile"/> (<c>Schemes:*:Profiles:Default</c>).</para>
/// </remarks>
public static class PlatformAuthenticationSchemes
{
    /// <summary>Profile mặc định trong <c>Authentication:Schemes:*:Profiles:{profile}</c> — không phải tên scheme.</summary>
    public const string DefaultProfile = "Default";

    /// <summary>Scheme API Key (<c>AddApiKeyInHeader</c>).</summary>
    public const string ApiKey = "ApiKey";

    /// <summary>Scheme JWT Bearer (<c>JwtBearerDefaults.AuthenticationScheme</c>).</summary>
    public const string Bearer = "Bearer";

    /// <summary>Scheme HTTP Basic (tên handler thường dùng trong ASP.NET Core).</summary>
    public const string Basic = "Basic";
}
