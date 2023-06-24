using Infrastructure.Database.Abstractions;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Jarvis.Core.Database.Repositories
{
    public interface IPermissionRepository : IRepository
    {
        Task<List<IdentityUserRole<Guid>>> FindRolesByUserAsync(Guid idUser);

        Task<List<IdentityUserClaim<Guid>>> FindUserClaimByUserAsync(Guid idUser);

        Task<List<IdentityRoleClaim<Guid>>> FindRoleClaimByRolesAsync(List<Guid> idRoles);

        Task<List<IdentityRoleClaim<Guid>>> FindRoleClaimByRoleAsync(Guid idRole);

        Task InsertUserRolesAsync(List<IdentityUserRole<Guid>> userRoles);

        Task InsertUserClaimsAsync(List<IdentityUserClaim<Guid>> claims);

        Task InsertRoleClaimsAsync(List<IdentityRoleClaim<Guid>> claims);

        void DeleteRoleClaims(List<IdentityRoleClaim<Guid>> claims);

        void DeleteUserRoles(List<IdentityUserRole<Guid>> userRoles);

        void DeleteUserClaims(List<IdentityUserClaim<Guid>> claims);
    }
}
