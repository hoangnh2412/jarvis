using System;
using System.Threading.Tasks;
using Jarvis.Core.Abstractions;
using Jarvis.Core.Models.Organizations;
using Jarvis.Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace Jarvis.Core.Controllers
{
    [Route("organizations")]
    [ApiController]
    public class OrganizationController : ControllerBase
    {
        private readonly IOrganizationService _organizationService;
        private readonly IWorkContext _workContext;

        public OrganizationController(
            IOrganizationService organizationService,
            IWorkContext workContext)
        {
            _organizationService = organizationService;
            _workContext = workContext;
        }

        [HttpGet]
        public async Task<IActionResult> PagingAsync([FromQuery] PagingOrganzationRequestModel paging)
        {
            var tenantCode = await _workContext.GetTenantCodeAsync();
            var userCode = _workContext.GetUserCode();

            var paged = await _organizationService.PaginationAsync(tenantCode, userCode, paging);
            return Ok(paged);
        }

        [HttpGet("{code}")]
        public async Task<IActionResult> GetOrganizationUnitInfoAsync([FromRoute] Guid code)
        {
            var organizationUnit = await _organizationService.GetUnitByCodeAsync(code);
            return Ok(organizationUnit);
        }

        [HttpPost]
        public async Task<IActionResult> PostOrganizationUnitAsync([FromBody] CreateOrganizationUnitRequestModel request)
        {
            var tenantCode = await _workContext.GetTenantCodeAsync();
            var userCode = _workContext.GetUserCode();

            var result = await _organizationService.CreateUnitAsync(tenantCode, userCode, request);
            if (!result)
                return Conflict();

            return Ok();
        }

        [HttpPut("{code}")]
        public async Task<IActionResult> PutOrganizationUnitAsync([FromRoute] Guid code, [FromBody] UpdateOrganizationUnitRequestModel request)
        {
            var tenantCode = await _workContext.GetTenantCodeAsync();
            var userCode = _workContext.GetUserCode();

            var result = await _organizationService.UpdateUnitAsync(tenantCode, userCode, code, request);
            if (!result)
                return NotFound();

            return Ok();
        }

        [HttpPut("{sourceCode}/move/{destCode}")]
        public IActionResult PutOrganizationTree([FromRoute] Guid sourceCode, [FromRoute] Guid destCode)
        {
            return Ok();
        }

        [HttpDelete("{code}")]
        public async Task<IActionResult> DeleteOrganizationUnitAsync([FromRoute] Guid code)
        {
            var tenantCode = await _workContext.GetTenantCodeAsync();
            var userCode = _workContext.GetUserCode();

            var result = await _organizationService.DeleteUnitAsync(tenantCode, userCode, code);
            if (!result)
                return NotFound();

            return Ok();
        }

        [HttpGet("{organizationCode}/users")]
        public async Task<IActionResult> GetOrganizationUsersAsync([FromRoute] Guid organizationCode)
        {
            var users = await _organizationService.GetUsersByUnitAsync(organizationCode);
            return Ok(users);
        }

        [HttpPost("{organizationCode}/users/{userCode}/{level}")]
        public async Task<IActionResult> PostOrganizationUserAsync([FromRoute] Guid organizationCode, [FromRoute] Guid userCode, [FromRoute] int level)
        {
            var result = await _organizationService.CreateUserAsync(new CreateOrganizationUserRequestModel
            {
                Level = level,
                OrganizationUnitCode = organizationCode,
                OrganizationUserCode = userCode
            });
            if (!result)
                return Conflict();

            return Ok();
        }

        [HttpDelete("{organizationCode}/users/{userCode}")]
        public async Task<IActionResult> DeleteOrganizationUserAsync([FromRoute] Guid organizationCode, [FromRoute] Guid userCode)
        {
            var result = await _organizationService.DeleteUserAsync(new DeleteOrganizationUserRequestModel
            {
                OrganizationUnitCode = organizationCode,
                OrganizationUserCode = userCode
            });
            if (!result)
                return NotFound();

            return Ok();
        }
    }
}