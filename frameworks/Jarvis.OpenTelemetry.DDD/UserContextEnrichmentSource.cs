using Jarvis.DDD.Domain.Services;
using Jarvis.OpenTelemetry.Abstractions;
using Jarvis.OpenTelemetry.SemanticConventions;

namespace Jarvis.OpenTelemetry.DDD;

/// <summary>
/// Maps <see cref="ICurrentUser{TUser}"/> / <see cref="ICurrentTenant{TTenant}"/> into OpenTelemetry <see cref="UserAttributes"/>.
/// </summary>
public sealed class UserContextEnrichmentSource<TUser, TTenant>(
    ICurrentUser<TUser> currentUser,
    ICurrentTenant<TTenant> currentTenant) : IEnrichmentSource
    where TUser : class, ICurrentUserIdentity
    where TTenant : class, ICurrentTenantIdentity
{
    public async Task<Dictionary<string, string>> ExtractAsync()
    {
        var user = await currentUser.GetAsync().ConfigureAwait(false);
        var userId = user?.UserId ?? Guid.Empty;
        // CurrentUserInfo (UserName) sống ở Authentication — OTEL.DDD chỉ contract Domain.
        const string userName = "anonymous";
        var tenantId = await currentTenant.GetIdAsync().ConfigureAwait(false) ?? Guid.Empty;

        return new Dictionary<string, string>
        {
            { UserAttributes.Id, userId.ToString() },
            { UserAttributes.UserName, userName },
            { UserAttributes.TenantId, tenantId.ToString() },
        };
    }
}
