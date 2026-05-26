using System.Security.Claims;

namespace Jarvis.Authentication.Basic;

/// <summary>Kết quả validate credential Basic — chứa username và claims gán cho principal.</summary>
public sealed class BasicValidationResult
{
    public required string Username { get; init; }

    public IReadOnlyList<Claim> Claims { get; init; } = [];
}

/// <summary>
/// Extension point validate username/password cho Basic Authentication.
/// </summary>
/// <remarks>
/// <para><b>Chức năng:</b> nhận scheme name + credential, trả <see cref="BasicValidationResult"/> hoặc <c>null</c> nếu sai.</para>
/// <para><b>Khi nào dùng:</b> override mặc định <see cref="ConfigBasicCredentialValidator"/> khi credential
/// không nằm trong config (DB, identity provider). Hoặc dùng <c>AddCoreBasic(configuration, lookup)</c>
/// với <see cref="BasicCredentialLookupAsync"/> nếu chỉ cần tra user từ nguồn ngoài.</para>
/// </remarks>
public interface IBasicCredentialValidator
{
    Task<BasicValidationResult?> ValidateAsync(
        string schemeName,
        string username,
        string password,
        CancellationToken cancellationToken = default);
}
