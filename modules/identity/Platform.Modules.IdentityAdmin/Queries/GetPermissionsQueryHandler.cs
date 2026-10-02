namespace Platform.Modules.IdentityAdmin.Queries;

using Platform.Authentication;
using Platform.DDD.Application.Contracts.Queries;
using Platform.DDD.Domain.Services;
using Platform.Authorization.Abstractions;

public sealed class GetPermissionsQueryHandler : IAsyncQueryHandler<GetPermissionsQuery, IReadOnlyCollection<string>>
{
    private readonly IPermissionProvider _permissionProvider;
    private readonly ICurrentUser<CurrentUserInfo> _currentUser;

    public GetPermissionsQueryHandler(
        IPermissionProvider permissionProvider,
        ICurrentUser<CurrentUserInfo> currentUser)
    {
        _permissionProvider = permissionProvider ?? throw new ArgumentNullException(nameof(permissionProvider));
        _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
    }

    public async Task<IReadOnlyCollection<string>> HandleAsync(GetPermissionsQuery query, CancellationToken cancellationToken = default)
    {
        var actor = await _currentUser.GetAsync(cancellationToken);
        if (actor?.UserId == null)
            throw new UnauthorizedAccessException("Current user not found");

        var permissions = await _permissionProvider.GetPermissionsAsync(actor.UserId.Value, cancellationToken);
        return permissions.ToList();
    }
}
