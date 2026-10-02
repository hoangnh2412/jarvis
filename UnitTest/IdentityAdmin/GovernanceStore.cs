namespace UnitTest.IdentityAdmin;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Platform.Authentication.Identity.Domain;
using Platform.Authorization.EntityFramework.Persistence;
using Platform.Authorization.Services;
using Platform.Authorization.EntityFramework.Services;

/// <summary>
/// InMemory identity store so handlers run against the real RoleGovernanceValidator / PermissionProvider.
/// Roles added here must be the same instances returned by the mocked RoleManager.
/// </summary>
internal sealed class GovernanceStore : IAsyncDisposable
{
    private sealed class StoreContext(DbContextOptions options) : IdentityDbContextBase(options);

    public GovernanceStore()
    {
        Context = new StoreContext(new DbContextOptionsBuilder<StoreContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);
    }

    public IdentityDbContextBase Context { get; }

    public RoleGovernanceValidator Validator => new(Context);

    public PermissionProvider PermissionProvider => new(Context);

    public Role AddRole(string name, int level, bool isSystem = false, params string[] permissions)
    {
        var role = new Role
        {
            Id = Guid.NewGuid(),
            Name = name,
            NormalizedName = name.ToUpperInvariant(),
            RoleLevel = level,
            IsSystemRole = isSystem
        };
        Context.Roles.Add(role);
        foreach (var permission in permissions)
            Context.RoleClaims.Add(new IdentityRoleClaim<Guid> { RoleId = role.Id, ClaimType = "Permission", ClaimValue = permission });
        Context.SaveChanges();
        return role;
    }

    public Guid AddUser(params Role[] roles)
    {
        var userId = Guid.NewGuid();
        foreach (var role in roles)
            Context.UserRoles.Add(new IdentityUserRole<Guid> { UserId = userId, RoleId = role.Id });
        Context.SaveChanges();
        return userId;
    }

    public ValueTask DisposeAsync() => Context.DisposeAsync();
}
