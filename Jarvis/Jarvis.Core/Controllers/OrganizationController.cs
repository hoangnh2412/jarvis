using System;
using System.Threading.Tasks;
using Infrastructure.Database.Models;
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
        public async Task<IActionResult> GetTreeAsync()
        {
            var tenantCode = await _workContext.GetTenantCodeAsync();
            var userCode = _workContext.GetUserCode();

            var tree = await _organizationService.GetTreeAsync(tenantCode);
            return Ok(tree);
        }

        [HttpGet("{code}")]
        public async Task<IActionResult> GetUnitInfoAsync([FromRoute] Guid code)
        {
            var organizationUnit = await _organizationService.GetUnitByCodeAsync(code);
            return Ok(organizationUnit);
        }

        [HttpPost]
        public async Task<IActionResult> PostUnitAsync([FromBody] CreateOrganizationUnitRequestModel request)
        {
            var tenantCode = await _workContext.GetTenantCodeAsync();
            var userCode = _workContext.GetUserCode();

            var code = Guid.NewGuid();

            bool result = false;
            if (request.IdParent.Value != Guid.Empty)
                result = await _organizationService.InsertNodeAsync(tenantCode, userCode, code, request);
            else
                result = await _organizationService.InsertRootAsync(tenantCode, userCode, code, request);

            if (!result)
                return Conflict();

            return Ok(code);
        }

        [HttpPut("{code}")]
        public async Task<IActionResult> PutUnitAsync([FromRoute] Guid code, [FromBody] UpdateOrganizationUnitRequestModel request)
        {
            var tenantCode = await _workContext.GetTenantCodeAsync();
            var userCode = _workContext.GetUserCode();

            var result = await _organizationService.UpdateUnitAsync(tenantCode, userCode, code, request);
            if (!result)
                return NotFound();

            return Ok();
        }

        [HttpPut("{code}/move")]
        public async Task<IActionResult> MoveUnitAsync([FromRoute] Guid code, [FromQuery] Guid? parentCode, [FromQuery] Guid? leftCode)
        {
            var tenantCode = await _workContext.GetTenantCodeAsync();
            var userCode = _workContext.GetUserCode();

            var result = await _organizationService.MoveNodeAsync(tenantCode, userCode, code, new MoveNodeRequestModel
            {
                LeftNode = leftCode,
                ParentNode = parentCode,
                RightNode = null
            });

            if (!result)
                return NotFound();

            return Ok();
        }

        [HttpDelete("{code}")]
        public async Task<IActionResult> DeleteOrganizationUnitAsync([FromRoute] Guid code)
        {
            var tenantCode = await _workContext.GetTenantCodeAsync();
            var userCode = _workContext.GetUserCode();

            var result = await _organizationService.RemoveNodeAsync(tenantCode, userCode, code);
            if (!result)
                return NotFound();

            return Ok();
        }

        [HttpGet("users")]
        public async Task<IActionResult> GetUsersNotInUnitAsync([FromRoute] Guid code, [FromQuery] Paging paging)
        {
            var tenantCode = await _workContext.GetTenantCodeAsync();
            var userCode = _workContext.GetUserCode();

            var users = await _organizationService.GetUsersNotInUnitAsync(tenantCode, code, paging);
            return Ok(users);
        }

        [HttpGet("{code}/users")]
        public async Task<IActionResult> GetUsersInUnitAsync([FromRoute] Guid code, [FromQuery] Paging paging)
        {
            var tenantCode = await _workContext.GetTenantCodeAsync();
            var userCode = _workContext.GetUserCode();

            var users = await _organizationService.GetUsersInUnitAsync(tenantCode, code, paging);
            return Ok(users);
        }

        [HttpPost("{code}/users/{userCode}")]
        public async Task<IActionResult> PostOrganizationUserAsync([FromRoute] Guid code, [FromRoute] Guid userCode)
        {
            var result = await _organizationService.CreateUserAsync(new CreateOrganizationUserRequestModel
            {
                Level = 0,
                OrganizationUnitCode = code,
                OrganizationUserCode = userCode
            });
            if (!result)
                return Conflict();

            return Ok();
        }

        [HttpDelete("{code}/users/{userCode}")]
        public async Task<IActionResult> DeleteOrganizationUserAsync([FromRoute] Guid code, [FromRoute] Guid userCode)
        {
            var result = await _organizationService.DeleteUserAsync(new DeleteOrganizationUserRequestModel
            {
                OrganizationUnitCode = code,
                OrganizationUserCode = userCode
            });
            if (!result)
                return NotFound();

            return Ok();
        }
    }
}