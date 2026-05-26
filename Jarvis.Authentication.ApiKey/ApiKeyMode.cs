namespace Jarvis.Authentication.ApiKey;

/// <summary>
/// Định dạng giá trị API key trong header — cấu hình qua <c>Authentication:ApiKey:{realm}:Mode</c>
/// trên <see cref="AuthenticationApiKeyOption"/>.
/// </summary>
/// <remarks>
/// <see cref="ApiKeyProvider"/> dùng mode để quyết định cách parse và so khớp key.
/// Mỗi realm (section con dưới <c>Authentication:ApiKey</c>) có thể đặt mode riêng.
/// </remarks>
public enum ApiKeyMode
{
    /// <summary>
    /// Header chứa secret thuần — so khớp trực tiếp với <c>Keys[]</c> của scheme mặc định.
    /// </summary>
    /// <remarks>
    /// <para><b>Khi nào dùng:</b> một client, một integration đơn giản, hoặc khi không cần phân tách tenant/partner qua prefix.</para>
    /// <para><b>Ví dụ header:</b> <c>X-API-KEY: my-secret-key</c></para>
    /// <para>Giá trị có dạng <c>realm:secret</c> sẽ bị từ chối.</para>
    /// </remarks>
    SingleKey,

    /// <summary>
    /// Header chứa <c>{realm}:{secret}</c> — <c>realm</c> trùng tên section config, <c>secret</c> so với <c>Keys[]</c> của realm đó.
    /// </summary>
    /// <remarks>
    /// <para><b>Khi nào dùng:</b> nhiều tenant/partner trên cùng header name; mỗi realm có bộ key riêng (ví dụ <c>Default</c>, <c>Integration</c>).</para>
    /// <para><b>Ví dụ header:</b> <c>X-API-KEY: Default:my-secret-key</c>, <c>X-API-KEY: Integration:partner-key</c></para>
    /// <para>Secret không có prefix <c>realm:</c> sẽ bị từ chối (trừ khi realm đó cũng cấu hình <see cref="SingleKey"/>).</para>
    /// </remarks>
    RealmKey
}
