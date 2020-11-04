using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Infrastructure.Database.Models;
using Jarvis.Core.Models.Organizations;

namespace Jarvis.Core.Abstractions
{
    public interface IOrganizationService
    {
        Task<Paged<PagingOrganizationResponseModel>> PaginationAsync(Guid tenantCode, Guid userCode, PagingOrganzationRequestModel paging);

        Task<GetOrganizationUnitResponseModel> GetUnitByCodeAsync(Guid code);

        Task<bool> CreateUnitAsync(Guid tenantCode, Guid userCode, CreateOrganizationUnitRequestModel request);

        Task<bool> UpdateUnitAsync(Guid tenantCode, Guid userCode, Guid code, UpdateOrganizationUnitRequestModel request);

        Task<bool> DeleteUnitAsync(Guid tenantCode, Guid userCode, Guid code);

        Task<List<GetOrganizationUserResponseModel>> GetUsersByUnitAsync(Guid code);

        Task<bool> CreateUserAsync(CreateOrganizationUserRequestModel request);

        Task<bool> DeleteUserAsync(DeleteOrganizationUserRequestModel request);

        Task MoveUnitAsync(Guid sourceCode, Guid destCode);
    }
}