using System.Security.Claims;
using Jarvis.DDD.Domain.Services;
using Jarvis.DDD.Domain.Shared.Extensions;
using Microsoft.AspNetCore.Http;

namespace Jarvis.Authentication;

/// <summary>
/// Resolve <typeparamref name="TUser"/> qua <see cref="GetAsync"/>: ambient → user id từ token → store.
/// </summary>
public sealed class CurrentUser<TUser>(
    ICurrentUserAccessor<TUser> currentUserAccessor,
    IHttpContextAccessor httpContextAccessor,
    ICurrentUserStore<TUser> currentUserStore) : ICurrentUser<TUser>
    where TUser : class, ICurrentUserIdentity
{
    private TUser? _resolved;
    private bool _loaded;

    public async Task<TUser?> GetAsync(CancellationToken cancellationToken = default)
    {
        var ambient = currentUserAccessor.Current;
        if (ambient != null)
            return ambient;

        if (_loaded)
            return _resolved;

        var userId = ParseGuidClaim(ClaimTypes.NameIdentifier, "sub");
        if (!userId.HasValue)
        {
            _loaded = true;
            _resolved = null;
            return null;
        }

        _resolved = await currentUserStore
            .FindAsync(userId.Value, cancellationToken)
            .ConfigureAwait(false);
        _loaded = true;
        return _resolved;
    }

    private Guid? ParseGuidClaim(params string[] claimNames)
    {
        var user = httpContextAccessor.HttpContext?.User;
        if (user == null)
            return null;

        foreach (var name in claimNames)
        {
            var value = ClaimsPrincipalExtension.GetClaim(user.Claims, name)?.Value;
            if (Guid.TryParse(value, out var id))
                return id;
        }

        return null;
    }
}
