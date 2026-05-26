namespace Jarvis.Authentication.Basic;

/// <summary>
/// Tra cứu credential theo username từ nguồn ngoài config (DB, cache, vault, …).
/// </summary>
/// <param name="schemeName">Tên scheme Basic đang xác thực.</param>
/// <param name="username">Username từ header <c>Authorization: Basic</c>.</param>
/// <param name="cancellationToken">Token huỷ request.</param>
/// <returns><see cref="BasicUserCredential"/> nếu user tồn tại; <c>null</c> nếu không tìm thấy.</returns>
public delegate Task<BasicUserCredential?> BasicCredentialLookupAsync(
    string schemeName,
    string username,
    CancellationToken cancellationToken = default);
