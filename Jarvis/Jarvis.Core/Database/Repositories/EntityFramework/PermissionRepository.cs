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
        public async Task<List<IdentityUserRole<Guid>>> FindRolesByUserAsync(Guid userId)
        {
            IQueryable<IdentityUserRole<Guid>> query = StorageContext.Set<IdentityUserRole<Guid>>();
            return await query
                .Where(x => x.UserId == userId)
                .ToListAsync();
        }

        public async Task<List<IdentityUserClaim<Guid>>> FindUserClaimByUserAsync(Guid userId)
        {
            IQueryable<IdentityUserClaim<Guid>> query = StorageContext.Set<IdentityUserClaim<Guid>>();
            return await query.Where(x => x.UserId == userId).ToListAsync();
        }

        public async Task<List<IdentityRoleClaim<Guid>>> FindRoleClaimByRolesAsync(List<Guid> roleIds)
        {
            IQueryable<IdentityRoleClaim<Guid>> query = StorageContext.Set<IdentityRoleClaim<Guid>>();
            return await query.Where(x => roleIds.Contains(x.RoleId)).ToListAsync();
        }

        public async Task<List<IdentityRoleClaim<Guid>>> FindRoleClaimByRoleAsync(Guid roleId)
        {
            IQueryable<IdentityRoleClaim<Guid>> query = StorageContext.Set<IdentityRoleClaim<Guid>>();
            return await query.Where(x => x.RoleId == roleId).ToListAsync();
        }

        public async Task InsertUserRolesAsync(List<IdentityUserRole<Guid>> userRoles)
        {
            var dbset = StorageContext.Set<IdentityUserRole<Guid>>();
            await dbset.AddRangeAsync(userRoles);
        }

        public void DeleteUserRoles(List<IdentityUserRole<Guid>> userRoles)
        {
            var dbset = StorageContext.Set<IdentityUserRole<Guid>>();
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

        public void DeleteUserClaims(List<IdentityUserClaim<Guid>> claims)
        {
            var dbset = StorageContext.Set<IdentityUserClaim<Guid>>();
            dbset.RemoveRange(claims);
        }
    }
}
