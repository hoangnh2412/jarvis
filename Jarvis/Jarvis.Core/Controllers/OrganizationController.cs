using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Infrastructure.Database.Models;
using Jarvis.Core.Abstractions;
using Jarvis.Core.Models.Organizations;
using Jarvis.Core.Permissions;
using Jarvis.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Jarvis.Core.Controllers
{
    [Authorize]
    [Route("organizations")]
    [ApiController]
    public class OrganizationController : ControllerBase
    {
        public OrganizationController()
        {
        }

        [Authorize(nameof(CorePolicy.OrganizationPolicy.OrganizationUnit_Read))]
        [HttpGet]
        public async Task<IActionResult> GetTreeAsync(
            [FromServices] IOrganizationService organizationService,
            [FromServices] IWorkContext workContext
        )
        {
            var tenantCode = await workContext.GetTenantCodeAsync();
            var userCode = workContext.GetUserCode();

            var tree = await organizationService.GetTreeAsync(tenantCode);
            return Ok(tree);
        }

        [Authorize(nameof(CorePolicy.OrganizationPolicy.OrganizationUnit_Read))]
        [HttpGet("{code}")]
        public async Task<IActionResult> GetUnitInfoAsync(
            [FromRoute] Guid code,
            [FromServices] IOrganizationService organizationService,
            [FromServices] IWorkContext workContext)
        {
            var organizationUnit = await organizationService.GetUnitByCodeAsync(code);
            return Ok(organizationUnit);
        }

        [Authorize(nameof(CorePolicy.OrganizationPolicy.OrganizationUnit_Create))]
        [HttpPost]
        public async Task<IActionResult> PostUnitAsync(
            [FromBody] CreateOrganizationUnitRequestModel request,
            [FromServices] IOrganizationService organizationService,
            [FromServices] IWorkContext workContext)
        {
            var tenantCode = await workContext.GetTenantCodeAsync();
            var userCode = workContext.GetUserCode();

            var code = Guid.NewGuid();

            bool result = false;
            if (request.IdParent.Value != Guid.Empty)
                result = await organizationService.InsertNodeAsync(tenantCode, userCode, code, request);
            else
                result = await organizationService.InsertRootAsync(tenantCode, userCode, code, request);

            if (!result)
                return Conflict();

            return Ok(code);
        }

        [Authorize(nameof(CorePolicy.OrganizationPolicy.OrganizationUnit_Update))]
        [HttpPut("{code}")]
        public async Task<IActionResult> PutUnitAsync(
            [FromRoute] Guid code, 
            [FromBody] UpdateOrganizationUnitRequestModel request,
            [FromServices] IOrganizationService organizationService,
            [FromServices] IWorkContext workContext)
        {
            var tenantCode = await workContext.GetTenantCodeAsync();
            var userCode = workContext.GetUserCode();

            var result = await organizationService.UpdateUnitAsync(tenantCode, userCode, code, request);
            if (!result)
                return NotFound();

            return Ok();
        }

        [Authorize(nameof(CorePolicy.OrganizationPolicy.OrganizationUnit_Update))]
        [HttpPut("{code}/move")]
        public async Task<IActionResult> MoveUnitAsync(
            [FromRoute] Guid code, 
            [FromQuery] Guid? parentCode, 
            [FromQuery] Guid? leftCode,
            [FromServices] IOrganizationService organizationService,
            [FromServices] IWorkContext workContext)
        {
            var tenantCode = await workContext.GetTenantCodeAsync();
            var userCode = workContext.GetUserCode();

            var result = await organizationService.MoveNodeAsync(tenantCode, userCode, code, new MoveNodeRequestModel
            {
                LeftNode = leftCode,
                ParentNode = parentCode,
                RightNode = null
            });

            if (!result)
                return NotFound();

            return Ok();
        }

        [Authorize(nameof(CorePolicy.OrganizationPolicy.OrganizationUnit_Delete))]
        [HttpDelete("{code}")]
        public async Task<IActionResult> DeleteOrganizationUnitAsync(
            [FromRoute] Guid code,
            [FromServices] IOrganizationService organizationService,
            [FromServices] IWorkContext workContext)
        {
            var tenantCode = await workContext.GetTenantCodeAsync();
            var userCode = workContext.GetUserCode();

            var result = await organizationService.RemoveNodeAsync(tenantCode, userCode, code);
            if (!result)
                return NotFound();

            return Ok();
        }

        [Authorize(nameof(CorePolicy.OrganizationPolicy.OrganizationUnit_Read))]
        [HttpGet("{code}/users")]
        public async Task<IActionResult> GetUsersNotInUnitAsync(
            [FromRoute] Guid code, 
            [FromQuery] Paging paging,
            [FromServices] IOrganizationService organizationService,
            [FromServices] IWorkContext workContext)
        {
            var tenantCode = await workContext.GetTenantCodeAsync();
            var userCode = workContext.GetUserCode();

            var users = await organizationService.GetUsersNotInUnitAsync(tenantCode, code, paging);
            return Ok(users);
        }

        [Authorize(nameof(CorePolicy.OrganizationPolicy.OrganizationUnit_Read))]
        [HttpGet("{code}/members")]
        public async Task<IActionResult> GetUsersInUnitAsync(
            [FromRoute] Guid code, 
            [FromQuery] Paging paging,
            [FromServices] IOrganizationService organizationService,
            [FromServices] IWorkContext workContext)
        {
            var tenantCode = await workContext.GetTenantCodeAsync();
            var userCode = workContext.GetUserCode();

            var users = await organizationService.GetUsersInUnitAsync(tenantCode, code, paging);
            return Ok(users);
        }

        [Authorize(nameof(CorePolicy.OrganizationPolicy.OrganizationUnit_Update))]
        [HttpPost("{code}/user/{userCode}")]
        public async Task<IActionResult> PostOrganizationUserAsync(
            [FromRoute] Guid code, 
            [FromRoute] Guid userCode,
            [FromServices] IOrganizationService organizationService,
            [FromServices] IWorkContext workContext)
        {
            var tenantCode = await workContext.GetTenantCodeAsync();

            var result = await organizationService.CreateUsersAsync(tenantCode, workContext.GetUserCode(), new CreateOrganizationUserRequestModel
            {
                UnitCode = code,
                UserCodes = new List<Guid> { userCode }
            });
            if (!result)
                return Conflict();

            return Ok();
        }

        [Authorize(nameof(CorePolicy.OrganizationPolicy.OrganizationUnit_Update))]
        [HttpPost("{code}/users")]
        public async Task<IActionResult> PostOrganizationUsersAsync(
            [FromRoute] Guid code, 
            [FromBody] List<Guid> userCodes,
            [FromServices] IOrganizationService organizationService,
            [FromServices] IWorkContext workContext)
        {
            var tenantCode = await workContext.GetTenantCodeAsync();
            var userCode = workContext.GetUserCode();

            var result = await organizationService.CreateUsersAsync(code, userCode, new CreateOrganizationUserRequestModel
            {
                UnitCode = code,
                UserCodes = userCodes
            });
            if (!result)
                return Conflict();

            return Ok();
        }

        [Authorize(nameof(CorePolicy.OrganizationPolicy.OrganizationUnit_Update))]
        [HttpDelete("{code}/user/{userCode}")]
        public async Task<IActionResult> DeleteOrganizationUserAsync(
            [FromRoute] Guid code, 
            [FromRoute] Guid userCode,
            [FromServices] IOrganizationService organizationService,
            [FromServices] IWorkContext workContext)
        {
            var result = await organizationService.DeleteUserAsync(new DeleteOrganizationUserRequestModel
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