using Infrastructure.Database.Abstractions;
using Infrastructure.Database.Entities;
using Infrastructure.Database.EntityFramework;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Jarvis.Core.Database.Repositories.EntityFramework
{
    public class PermissionRepository : EntityRepository<User>, IPermissionRepository
    {
        public async Task<List<IdentityUserRole<Guid>>> FindRolesByUserAsync(Guid idUser)
        {
            IQueryable<IdentityUserRole<Guid>> query = StorageContext.Set<IdentityUserRole<Guid>>();
            var roles = await query
                .Where(x => x.UserId == idUser)
                .ToListAsync();
            return roles;
        }

        public async Task<List<IdentityUserClaim<Guid>>> FindUserClaimByUserAsync(Guid idUser)
        {
            IQueryable<IdentityUserClaim<Guid>> query = StorageContext.Set<IdentityUserClaim<Guid>>();
            var claims = await query.Where(x => x.UserId == idUser).ToListAsync();
            return claims;
        }

        public async Task<List<IdentityRoleClaim<Guid>>> FindRoleClaimByRolesAsync(List<Guid> idRoles)
        {
            IQueryable<IdentityRoleClaim<Guid>> query = StorageContext.Set<IdentityRoleClaim<Guid>>();
            var claims = await query.Where(x => idRoles.Contains(x.RoleId)).ToListAsync();
            return claims;
        }

        public async Task<List<IdentityRoleClaim<Guid>>> FindRoleClaimByRoleAsync(Guid idRole)
        {
            IQueryable<IdentityRoleClaim<Guid>> query = StorageContext.Set<IdentityRoleClaim<Guid>>();
            var claims = await query.Where(x => x.RoleId == idRole).ToListAsync();
            return claims;
        }

        public async Task InsertUserRolesAsync(List<IdentityUserRole<Guid>> userRoles)
        {
            var dbset = StorageContext.Set<IdentityUserRole<Guid>>();
            await dbset.AddRangeAsync(userRoles);
        }

        public void DeleteUserRoles(List<IdentityUserRole<Guid>> userRoles)
        {
            var dbset = StorageContext.Set<IdentityUserRole<Guid>>();
            //var userRoles = dbset.Where(x => x.UserId == idUser && idRoles.Contains(x.RoleId)).ToList();

            dbset.RemoveRange(userRoles);
        }

        public async Task InsertUserClaimsAsync(List<IdentityUserClaim<Guid>> claims)
        {
            var dbset = StorageContext.Set<IdentityUserClaim<Guid>>();
            await dbset.AddRangeAsync(claims);
        }

        public async Task InsertRoleClaimsAsync(List<IdentityRoleClaim<Guid>> claims)
        {
            var dbset = StorageContext.Set<IdentityRoleClaim<Guid>>();
            await dbset.AddRangeAsync(claims);
        }

        public void DeleteRoleClaims(List<IdentityRoleClaim<Guid>> claims)
        {
            var dbset = StorageContext.Set<IdentityRoleClaim<Guid>>();
            dbset.RemoveRange(claims);
        }
    }
}
