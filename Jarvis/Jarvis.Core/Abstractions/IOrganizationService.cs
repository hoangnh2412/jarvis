using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Infrastructure.Database.Models;
using Jarvis.Core.Models.Organizations;

namespace Jarvis.Core.Abstractions
{
    public interface IOrganizationService
    {
        Task<List<GetOrganizationUnitResponseModel>> GetAllAsync(Guid tenantCode);

        Task<List<GetTreeNodeResponseModel>> GetTreeAsync(Guid tenantCode);

        Task<Paged<PagingOrganizationResponseModel>> PaginationAsync(Guid tenantCode, Guid userCode, PagingOrganzationRequestModel paging);

        Task<GetOrganizationUnitResponseModel> GetUnitByCodeAsync(Guid code);

        Task<bool> UpdateUnitAsync(Guid tenantCode, Guid userCode, Guid code, UpdateOrganizationUnitRequestModel request);

        Task<bool> RemoveNodeAsync(Guid tenantCode, Guid userCode, Guid code);

        Task<Paged<OrganizationUserInfoModel>> GetUsersInUnitAsync(Guid tenantCode, Guid code, Paging paging);

        Task<Paged<OrganizationUserInfoModel>> GetUsersNotInUnitAsync(Guid tenantcode, Guid code, Paging paging);

        Task<bool> CreateUsersAsync(Guid tenantCode, Guid userCode, CreateOrganizationUserRequestModel request);

        Task<bool> DeleteUserAsync(DeleteOrganizationUserRequestModel request);

        Task<bool> MoveNodeAsync(Guid tenantCode, Guid userCode, Guid sourceCode, MoveNodeRequestModel request);

        Task<bool> InsertNodeAsync(Guid tenantCode, Guid userCode, Guid code, CreateOrganizationUnitRequestModel request);

        Task<bool> InsertRootAsync(Guid tenantCode, Guid userCode, Guid code, CreateOrganizationUnitRequestModel request);
    }
}