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
        public async Task<User> FindUserByIdAsync(Guid tenantCode, Guid id)
        {
            IQueryable<User> query = StorageContext.Set<User>();
            query = query.Where(x => x.Id == id && x.TenantCode == tenantCode);
            query = query.QueryByDeletedBy();

            return await query.Take(1).AsQueryable().FirstOrDefaultAsync();
        }

        public async Task<User> FindUserByIdAsync(ContextModel contextModel, Guid id)
        {
            IQueryable<User> query = StorageContext.Set<User>().Where(x => x.Id == id);
            //query = query.QueryByPermission(contextModel);
            query = query.QueryByTenantCode(contextModel.TenantCode);
            query = query.QueryByDeletedBy();

            return await query.Take(1).AsQueryable().FirstOrDefaultAsync();
        }

        public async Task<User> FindUserByUsernameAsync(Guid idTenant, string username)
        {
            IQueryable<User> query = StorageContext.Set<User>();
            query = query.Where(x => x.TenantCode == idTenant && x.NormalizedUserName == username);
            query = query.QueryByDeletedBy();

            return await query.Take(1).AsQueryable().FirstOrDefaultAsync();
        }

        public async Task<User> FindUserByUsernameAsync(string username)
        {
            IQueryable<User> query = StorageContext.Set<User>();
            query = query.Where(x => x.NormalizedUserName == username);
            query = query.QueryByDeletedBy();

            return await query.Take(1).AsQueryable().FirstOrDefaultAsync();
        }

        public async Task<UserInfo> FindUserInfoByIdAsync(Guid id)
        {
            IQueryable<UserInfo> query = StorageContext.Set<UserInfo>();
            var info = await query.FirstOrDefaultAsync(x => x.Id == id);
            return info;
        }

        public async Task<IEnumerable<UserInfo>> FindInfoByIdsAsync(List<Guid> ids)
        {
            IQueryable<UserInfo> query = StorageContext.Set<UserInfo>();
            var info = await query.Where(x => ids.Contains(x.Id)).ToListAsync();
            return info;
        }

        public async Task<Paged<User>> PagingAsync(ContextModel contextModel, Paging paging)
        {
            IQueryable<User> query = StorageContext.Set<User>();

            if (contextModel.Session.Claims.ContainsKey(nameof(SpecialPolicy.Special_DoEnything)))
                query = query.Where(x => x.Id != contextModel.IdUser);
            else
                query = query.Where(x => x.CreatedBy != Guid.Empty && x.Id != contextModel.IdUser && x.NormalizedUserName != "ROOT");

            //query = query.QueryByPermission(contextModel);
            query = query.QueryByTenantCode(contextModel.TenantCode);
            query = query.QueryByDeletedBy();

            if (!string.IsNullOrEmpty(paging.Q))
            {
                var q = paging.Q.ToUpper();
                query = query.Where(x => x.NormalizedUserName.Contains(q)
                                    || x.NormalizedEmail.Contains(q)
                                    || x.PhoneNumber.Contains(paging.Q));
            }

            return await query.ToPaginationAsync(paging);
        }

        public async Task AssignRoleToUserAsync(Guid idUser, Guid idRole)
        {
            var dbset = StorageContext.Set<IdentityUserRole<Guid>>();
            await dbset.AddAsync(new IdentityUserRole<Guid>
            {
                RoleId = idRole,
                UserId = idUser
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

            return await query.Take(1).AsQueryable().FirstOrDefaultAsync();
        }

        public async Task<List<IdentityUserRole<Guid>>> FindByIdRoleAsync(Guid idRole)
        {
            IQueryable<IdentityUserRole<Guid>> query = StorageContext.Set<IdentityUserRole<Guid>>();
            var user = await query.Where(x => x.RoleId == idRole).ToListAsync();

            return user;
        }
    }
}
