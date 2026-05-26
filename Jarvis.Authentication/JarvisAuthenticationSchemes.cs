namespace Jarvis.Authentication;

/// <summary>
/// Tên scheme authentication chuẩn của Jarvis — dùng thống nhất giữa config, code và <c>[Authorize]</c>.
/// </summary>
/// <remarks>
/// <para><b>Khi nào dùng:</b> tham chiếu scheme thay vì hard-code string — tránh lệch tên giữa
/// <c>appsettings</c>, <c>AddCoreApiKey</c> và authorization policy.</para>
/// </remarks>
public static class JarvisAuthenticationSchemes
{
    /// <summary>Policy scheme forward sang ApiKey/Basic/Bearer theo header.</summary>
    public const string Composite = "Composite";

    /// <summary>Scheme ApiKey mặc định — trùng section <c>Authentication:ApiKey:Default</c>.</summary>
    public const string ApiKey = "Default";

    /// <summary>Scheme HTTP Basic mặc định.</summary>
    public const string Basic = "Basic";
}
