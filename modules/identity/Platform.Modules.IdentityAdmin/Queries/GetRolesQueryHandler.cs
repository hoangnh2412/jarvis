namespace Platform.Modules.IdentityAdmin.Queries;

using Microsoft.AspNetCore.Identity;
using Platform.DDD.Application.Contracts.Queries;
using Platform.Authentication.Identity.Domain;

public sealed class GetRolesQueryHandler : IAsyncQueryHandler<GetRolesQuery, IReadOnlyCollection<RoleSummaryDto>>
{
    private readonly RoleManager<Role> _roleManager;

    public GetRolesQueryHandler(RoleManager<Role> roleManager)
    {
        _roleManager = roleManager ?? throw new ArgumentNullException(nameof(roleManager));
    }

    public async Task<IReadOnlyCollection<RoleSummaryDto>> HandleAsync(GetRolesQuery query, CancellationToken cancellationToken = default)
    {
        var roles = await _roleManager.Roles.MaterializeAsync(cancellationToken);

        return roles.Select(role => new RoleSummaryDto
        {
            RoleId = role.Id,
            Name = role.Name ?? string.Empty,
            DisplayName = role.DisplayName,
            RoleLevel = role.RoleLevel,
            IsSystemRole = role.IsSystemRole
        }).ToList();
    }
}
