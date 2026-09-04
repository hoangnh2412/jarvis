namespace Jarvis.Authentication.Basic;

/// <summary>
/// Nhà cung cấp xác thực Basic — config, database, object storage, hoặc nguồn tùy chỉnh.
/// </summary>
/// <remarks>
/// <para><b>Chức năng:</b> tra user và validate password theo nguồn của host; trả <see cref="BasicValidationResult"/> khi thành công.</para>
/// <para><b>Khi nào dùng:</b> implement <c>AddCoreBasic&lt;TCredentialProvider&gt;</c> thay cho
/// <see cref="ConfigBasicCredentialProvider"/> mặc định đọc <c>appsettings</c>.</para>
/// </remarks>
public interface IBasicCredentialProvider
{
    /// <summary>Xác thực username/password; <c>null</c> nếu thất bại.</summary>
    Task<BasicValidationResult?> AuthenticateAsync(
        string schemeName,
        string username,
        string password,
        CancellationToken cancellationToken = default);
}
