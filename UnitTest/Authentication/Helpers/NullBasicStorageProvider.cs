using Platform.Authentication.Basic;

namespace UnitTest.Authentication.Helpers;

internal sealed class NullBasicStorageProvider : IBasicCredentialProvider
{
    public Task<BasicValidationResult?> AuthenticateAsync(
        string schemeName,
        string username,
        string password,
        CancellationToken cancellationToken = default) =>
        Task.FromResult<BasicValidationResult?>(null);
}
