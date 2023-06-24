using Infrastructure.Database.Entities;
using Infrastructure.Database.EntityFramework;
using Infrastructure.Database.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Jarvis.Core.Database.Poco;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Jarvis.Core.Models;
using Jarvis.Core.Permissions;

namespace Jarvis.Core.Database.Repositories.EntityFramework
{
    public class UserRepository : EntityRepository<User>, IUserRepository
    {
        public async Task<User> FindUserByKeyAsync(Guid tenantCode, Guid key)
        {
            IQueryable<User> query = StorageContext.Set<User>();
            query = query.Where(x => x.Key == key && x.TenantCode == tenantCode);
            query = query.QueryByDeletedBy();

            return await query.FirstOrDefaultAsync();
        }

        public async Task<User> FindUserByKeyAsync(ContextModel contextModel, Guid key)
        {
            IQueryable<User> query = StorageContext.Set<User>().Where(x => x.Key == key);
            query = query.QueryByPermission(contextModel);
            query = query.QueryByDeletedBy();

            return await query.FirstOrDefaultAsync();
        }

        public async Task<User> FindUserByUsernameAsync(Guid idTenant, string username)
        {
            IQueryable<User> query = StorageContext.Set<User>();
            query = query.Where(x => x.TenantCode == idTenant && x.NormalizedUserName == username);
            query = query.QueryByDeletedBy();

            return await query.FirstOrDefaultAsync();
        }

        public async Task<UserInfo> FindUserInfoByKeyAsync(Guid key)
        {
            IQueryable<UserInfo> query = StorageContext.Set<UserInfo>();
            var info = await query.FirstOrDefaultAsync(x => x.Key == key);
            return info;
        }

        public async Task<List<UserInfo>> FindUserInfoByKeysAsync(List<Guid> keys)
        {
            IQueryable<UserInfo> query = StorageContext.Set<UserInfo>();
            var info = await query.Where(x => keys.Contains(x.Key)).ToListAsync();
            return info;
        }

        public async Task<Paged<User>> PagingAsync(ContextModel context, Paging paging)
        {
            IQueryable<User> query = StorageContext.Set<User>();

            if (context.Session.Type == Constants.UserType.SuperAdmin)
                query = query.Where(x => x.Key != context.UserKey);
            else if (context.Session.Type == Constants.UserType.Admin)
                query = query.Where(x => x.Key != context.UserKey && x.Type != Constants.UserType.SuperAdmin.GetHashCode());
            else
                query = query.Where(x => x.Key != context.UserKey && x.Type != Constants.UserType.SuperAdmin.GetHashCode() && x.Type != Constants.UserType.Admin.GetHashCode());

            query = query.QueryByTenantCode(context.TenantKey);
            query = query.QueryByDeletedBy();

            if (!string.IsNullOrEmpty(paging.Q))
            {
                var q = paging.Q.ToUpper();
                query = query.Where(x => x.NormalizedUserName.Contains(q)
                                    || x.NormalizedEmail.Contains(q)
                                    || x.PhoneNumber.Contains(paging.Q));
            }

            query = query.OrderByDescending(x => x.CreatedAt);
            return await query.ToPaginationAsync(paging);
        }

        public async Task<Paged<User>> PagingWithoutSomeUsersAsync(Guid tenantCode, Paging paging, List<Guid> keys)
        {
            IQueryable<User> query = StorageContext.Set<User>();

            query = query.QueryByTenantCode(tenantCode);
            query = query.QueryByDeletedBy();

            query = query.Where(x => !keys.Contains(x.Key));

            if (!string.IsNullOrEmpty(paging.Q))
            {
                var q = paging.Q.ToUpper();
                query = query.Where(x =>
                    x.NormalizedUserName.Contains(q)
                    || x.NormalizedEmail.Contains(q)
                    || x.PhoneNumber.Contains(paging.Q)
                );
            }

            return await query.ToPaginationAsync(paging);
        }

        public async Task AssignRoleToUserAsync(Guid userKey, Guid rokeKey)
        {
            var dbset = StorageContext.Set<IdentityUserRole<Guid>>();
            await dbset.AddAsync(new IdentityUserRole<Guid>
            {
                RoleId = rokeKey,
                UserId = userKey
            });
        }

        public async Task InsertUserAsync(User user)
        {
            var dbset = StorageContext.Set<User>();
            await dbset.AddAsync(user);
        }

        public async Task InsertUserInfoAsync(UserInfo info)
        {
            var dbset = StorageContext.Set<UserInfo>();
            await dbset.AddAsync(info);
        }

        public void UpdateUserFields(User user, params KeyValuePair<Expression<Func<User, object>>, object>[] properties)
        {
            var entry = StorageContext.Entry(user);
            if (entry.State == EntityState.Detached)
                StorageContext.Attach(user);

            foreach (var property in properties)
            {
                entry.Property(property.Key).CurrentValue = property.Value;
                entry.Property(property.Key).IsModified = true;
            }
        }

        public void UpdateUserInfoFields(UserInfo info, params KeyValuePair<Expression<Func<UserInfo, object>>, object>[] properties)
        {
            var entry = StorageContext.Entry(info);
            if (entry.State == EntityState.Detached)
                StorageContext.Attach(info);

            foreach (var property in properties)
            {
                entry.Property(property.Key).CurrentValue = property.Value;
                entry.Property(property.Key).IsModified = true;
            }
        }

        public void DeleteUserInfo(UserInfo info)
        {
            var dbset = StorageContext.Set<UserInfo>();
            dbset.Remove(info);
        }

        public async Task<User> FindFirstUserCreatedAsync(Guid tenantCode)
        {
            IQueryable<User> query = StorageContext.Set<User>();
            var user = await query.Where(x => x.TenantCode == tenantCode)
                .OrderBy(x => x.CreatedAt)
                .FirstOrDefaultAsync();

            return user;
        }

        public async Task<User> FindByIdAsync(Guid id)
        {
            IQueryable<User> query = StorageContext.Set<User>();
            query = query.Where(x => x.Id == id);
            query = query.QueryByDeletedBy();

            return await query.FirstOrDefaultAsync();
        }

        public async Task<List<User>> FindUserByIdsAsync(List<Guid> ids)
        {
            IQueryable<User> query = StorageContext.Set<User>();
            query = query.Where(x => ids.Contains(x.Id));
            query = query.QueryByDeletedBy();

            return await query.ToListAsync();
        }

        public async Task<List<T>> FindByIdsAsync<T>(List<Guid> ids, Expression<Func<User, T>> fieldSelector)
        {
            IQueryable<User> query = StorageContext.Set<User>();
            query = query.Where(x => ids.Contains(x.Id));
            query = query.QueryByDeletedBy();

            return (await query.Select(x => x.UserName).AsQueryable().ToListAsync()) as List<T>;
        }

        public async Task<List<IdentityUserRole<Guid>>> FindByIdRoleAsync(Guid idRole)
        {
            IQueryable<IdentityUserRole<Guid>> query = StorageContext.Set<IdentityUserRole<Guid>>();
            var user = await query.Where(x => x.RoleId == idRole).ToListAsync();

            return user;
        }

        public async Task<List<IdentityUserClaim<Guid>>> GetUserClaimsAsync(Guid key)
        {
            var dbset = StorageContext.Set<IdentityUserClaim<Guid>>();
            return await dbset.Where(x => x.UserId == key).ToListAsync();
        }

        public async Task<bool> UserHasClaimAsync(Guid id, string claim, bool notracking = false)
        {
            var dbset = StorageContext.Set<IdentityUserClaim<Guid>>();
            IQueryable<IdentityUserClaim<Guid>> queryable = dbset;
            if (notracking)
                queryable = dbset.AsNoTracking();

            return await queryable.AnyAsync(x => x.UserId == id && x.ClaimType == claim);
        }

        public async Task AssignClaimToUserAsync(Guid idUser, Dictionary<string, string> claims)
        {
            var dbset = StorageContext.Set<IdentityUserClaim<Guid>>();
            await dbset.AddRangeAsync(claims.Select(x => new IdentityUserClaim<Guid>
            {
                UserId = idUser,
                ClaimType = x.Key,
                ClaimValue = x.Value
            }));
        }

        public void DeleteUserClaim(List<IdentityUserClaim<Guid>> claims)
        {
            var dbset = StorageContext.Set<IdentityUserClaim<Guid>>();
            dbset.RemoveRange(claims);
        }
    }
}
