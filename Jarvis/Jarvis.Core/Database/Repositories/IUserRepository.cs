using Infrastructure.Database.Abstractions;
using Infrastructure.Database.Entities;
using Infrastructure.Database.Models;
using Jarvis.Core.Database.Poco;
using Jarvis.Core.Models;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Jarvis.Core.Database.Repositories
{
    public interface IUserRepository : IRepository<User>
    {
        Task<Paged<User>> PagingAsync(ContextModel context, Paging paging);

        Task<User> FindUserByIdAsync(ContextModel context, Guid id);

        Task<User> FindUserByIdAsync(Guid tenantCode, Guid id);

        Task<User> FindUserByUsernameAsync(Guid tenantCode, string username);

        Task<User> FindUserByUsernameAsync(string username);

        Task<UserInfo> FindUserInfoByIdAsync(Guid code);

        Task<IEnumerable<UserInfo>> FindInfoByIdsAsync(List<Guid> ids);
        
        Task AssignRoleToUserAsync(Guid idUser, Guid idRole);

        Task InsertUserAsync(User user);

        Task InsertUserInfoAsync(UserInfo info);

        void UpdateUserFields(User user, params KeyValuePair<Expression<Func<User, object>>, object>[] properties);

        void UpdateUserInfoFields(UserInfo user, params KeyValuePair<Expression<Func<UserInfo, object>>, object>[] properties);

        void DeleteUserInfo(UserInfo info);
        
        /// <summary>
        /// lấy tk đc tạo đầu tiên
        /// </summary>
        /// <param name="tenantCode"></param>
        /// <returns></returns>
        Task<User> FindFirstUserCreatedAsync(Guid tenantCode);

        /// <summary>
        /// lấy tài khoản theo code
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<User> FindByIdAsync(Guid id);

        /// <summary>
        /// lấy các tài khoản đc gán quyền theo idRole
        /// </summary>
        /// <param name="idRole"></param>
        /// <returns></returns>
        Task<List<IdentityUserRole<Guid>>> FindByIdRoleAsync(Guid idRole);
    }
}
