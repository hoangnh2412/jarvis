using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Infrastructure.Database.Models;
using Infrastructure.Extensions;
using Jarvis.Core.Abstractions;
using Jarvis.Core.Database;
using Jarvis.Core.Database.Poco;
using Jarvis.Core.Database.Repositories;
using Jarvis.Core.Models.Organizations;
using Microsoft.EntityFrameworkCore;

namespace Jarvis.Core.Services
{
    public class OrganizationService : IOrganizationService
    {
        private readonly ICoreUnitOfWork _uowCore;

        public OrganizationService(
            ICoreUnitOfWork uowCore)
        {
            _uowCore = uowCore;
        }

        public async Task<bool> CreateUnitAsync(Guid tenantCode, Guid userCode, CreateOrganizationUnitRequestModel request)
        {
            var repoOrganizationUnit = _uowCore.GetRepository<IOrganizationUnitRepository>();
            if (await repoOrganizationUnit.AnyAsync(x => x.Name == request.Name))
                return false;

            await repoOrganizationUnit.InsertAsync(new OrganizationUnit
            {
                Code = Guid.NewGuid(),
                CreatedAt = DateTime.Now,
                CreatedAtUtc = DateTime.UtcNow,
                CreatedBy = userCode,
                DeletedAt = null,
                DeletedAtUtc = null,
                DeletedBy = null,
                DeletedVersion = null,
                Description = request.Description,
                FullName = request.FullName,
                IdParent = request.IdParent,
                Name = request.Name,
                TenantCode = tenantCode,
                UpdatedAt = null,
                UpdatedAtUtc = null,
                UpdatedBy = null
            });
            await _uowCore.CommitAsync();
            return true;
        }

        public async Task<bool> CreateUserAsync(CreateOrganizationUserRequestModel request)
        {
            var repoOrganizationUser = _uowCore.GetRepository<IOrganizationUserRepository>();
            if (await repoOrganizationUser.AnyAsync(x => x.OrganizationCode == request.OrganizationUnitCode && x.IdUser == request.OrganizationUserCode))
                return false;

            await repoOrganizationUser.InsertAsync(new OrganizationUser
            {
                IdUser = request.OrganizationUserCode,
                Level = request.Level,
                OrganizationCode = request.OrganizationUnitCode
            });
            await _uowCore.CommitAsync();
            return true;
        }

        public async Task<bool> DeleteUnitAsync(Guid tenantCode, Guid userCode, Guid code)
        {
            var repoOrganizationUnit = _uowCore.GetRepository<IOrganizationUnitRepository>();
            var organizationUnit = await repoOrganizationUnit.GetQuery().FirstOrDefaultAsync(x => x.Code == code);
            if (organizationUnit == null)
                return false;

            repoOrganizationUnit.UpdateFields(organizationUnit,
                organizationUnit.Set(x => x.DeletedAt, DateTime.Now),
                organizationUnit.Set(x => x.DeletedAtUtc, DateTime.UtcNow),
                organizationUnit.Set(x => x.DeletedBy, userCode),
                organizationUnit.Set(x => x.DeletedVersion, organizationUnit.Id)
            );
            await _uowCore.CommitAsync();
            return true;
        }

        public async Task<bool> DeleteUserAsync(DeleteOrganizationUserRequestModel request)
        {
            var repoOrganizationUser = _uowCore.GetRepository<IOrganizationUserRepository>();
            var organizationUser = await repoOrganizationUser.GetQuery().FirstOrDefaultAsync(x => x.OrganizationCode == request.OrganizationUnitCode && x.IdUser == request.OrganizationUserCode);
            if (organizationUser == null)
                return false;

            repoOrganizationUser.Delete(organizationUser);
            await _uowCore.CommitAsync();
            return true;
        }

        public async Task<GetOrganizationUnitResponseModel> GetUnitByCodeAsync(Guid code)
        {
            var repoOrganizationUnit = _uowCore.GetRepository<IOrganizationUnitRepository>();
            var organizationUnit = await repoOrganizationUnit.GetQuery().FirstOrDefaultAsync(x => x.Code == code);
            return new GetOrganizationUnitResponseModel
            {
                Code = organizationUnit.Code,
                Description = organizationUnit.Description,
                FullName = organizationUnit.FullName,
                IdParent = organizationUnit.IdParent,
                Name = organizationUnit.Name,
                TenantCode = organizationUnit.TenantCode
            };
        }

        public async Task<List<GetOrganizationUserResponseModel>> GetUsersByUnitAsync(Guid code)
        {
            var repoOrganizationUser = _uowCore.GetRepository<IOrganizationUserRepository>();
            var users = await repoOrganizationUser.GetUsersByOrganizationAsync(code);

            var userCodes = users.Select(x => x.IdUser).ToList();
            var repoUser = _uowCore.GetRepository<IUserRepository>();
            var userInfos = await repoUser.FindUserInfoByIdsAsync(userCodes);
            var indexUserInfo = userInfos.ToDictionary(x => x.Id, x => x);

            var items = new List<GetOrganizationUserResponseModel>();
            foreach (var user in users)
            {
                var item = new GetOrganizationUserResponseModel
                {
                    Id = user.Id,
                    Level = user.Level,
                    OrganizationCode = user.OrganizationCode,
                    UserCode = user.IdUser
                };

                if (indexUserInfo.ContainsKey(user.IdUser))
                {
                    var userInfo = indexUserInfo[user.IdUser];
                    item.FullName = userInfo.FullName;
                    item.Avatar = userInfo.AvatarPath;
                }
                items.Add(item);
            }

            return items;
        }

        public Task MoveUnitAsync(Guid sourceCode, Guid destCode)
        {
            var repoUnit = _uowCore.GetRepository<IOrganizationUnitRepository>();
            return null;
        }

        public async Task<Paged<PagingOrganizationResponseModel>> PaginationAsync(Guid tenantCode, Guid userCode, PagingOrganzationRequestModel paging)
        {
            var repoOrganizationUnit = _uowCore.GetRepository<IOrganizationUnitRepository>();
            var paged = await repoOrganizationUnit.PagingAsync(tenantCode, userCode, paging);

            return new Paged<PagingOrganizationResponseModel>
            {
                Data = paged.Data.Select(x => new PagingOrganizationResponseModel
                {
                    Code = x.Code,
                    Description = x.Description,
                    FullName = x.FullName,
                    IdParent = x.IdParent,
                    Name = x.Name,
                    TenantCode = x.TenantCode
                }),
                Page = paged.Page,
                Q = paged.Q,
                Size = paged.Size,
                TotalItems = paged.TotalItems,
                TotalPages = paged.TotalPages
            };
        }

        public async Task<bool> UpdateUnitAsync(Guid tenantCode, Guid userCode, Guid code, UpdateOrganizationUnitRequestModel request)
        {
            var repoOrganizationUnit = _uowCore.GetRepository<IOrganizationUnitRepository>();
            var organizationUnit = await repoOrganizationUnit.GetQuery().FirstOrDefaultAsync(x => x.Code == code);
            if (organizationUnit == null)
                return false;

            repoOrganizationUnit.UpdateFields(organizationUnit,
                organizationUnit.Set(x => x.FullName, request.FullName),
                organizationUnit.Set(x => x.Name, request.Name),
                organizationUnit.Set(x => x.UpdatedAt, DateTime.Now),
                organizationUnit.Set(x => x.UpdatedAtUtc, DateTime.UtcNow),
                organizationUnit.Set(x => x.UpdatedBy, userCode),
                organizationUnit.Set(x => x.IdParent, request.IdParent),
                organizationUnit.Set(x => x.Description, request.Description)
            );

            await _uowCore.CommitAsync();
            return true;
        }
    }
}