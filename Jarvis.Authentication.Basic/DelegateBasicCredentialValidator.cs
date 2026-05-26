namespace Jarvis.Authentication.Basic;

/// <summary>
/// Validator tra credential qua <see cref="BasicCredentialLookupAsync"/> — host cung cấp nguồn DB/cache/vault bằng delegate.
/// </summary>
/// <remarks>
/// <para><b>Chức năng:</b> gọi delegate lookup theo username, so password và build claims.</para>
/// <para><b>Khi nào dùng:</b> đăng ký qua <c>AddCoreBasic(configuration, lookup)</c> khi user không nằm trong <c>appsettings</c>.</para>
/// </remarks>
public sealed class DelegateBasicCredentialValidator(BasicCredentialLookupAsync lookup) : IBasicCredentialValidator
{
    public async Task<BasicValidationResult?> ValidateAsync(
        string schemeName,
        string username,
        string password,
        CancellationToken cancellationToken = default)
    {
        var credential = await lookup(schemeName, username, cancellationToken);
        if (credential is null)
            return null;

        return BasicCredentialValidation.Validate(username, password, credential);
    }
}
