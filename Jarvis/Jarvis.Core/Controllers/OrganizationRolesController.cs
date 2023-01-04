using Infrastructure.Database.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Jarvis.Core.Database;
using System;
using Jarvis.Core.Permissions;
using Jarvis.Core.Database.Repositories;
using Jarvis.Core.Database.Poco;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Jarvis.Core.Controllers
{
    [Authorize(nameof(CorePolicy.OrganizationPolicy.OrganizationUnit_Roles))]
    [Route("organizations/{code}/roles")]
    [ApiController]
    public class OrganizationRolesController : ControllerBase
    {
        [HttpGet]
        public IActionResult GetUsers(
            [FromRoute] Guid code,
            [FromQuery] Paging paging,
            [FromServices] ICoreUnitOfWork uow)
        {
            var repo = uow.GetRepository<IOrganizationRoleRepository>();
            var users = repo.GetRolesByOrganization(code);
            return Ok(users);
        }

        [HttpPost]
        public async Task<IActionResult> PostUsersAsync(
            [FromRoute] Guid code,
            [FromBody] List<Guid> idUsers,
            [FromServices] ICoreUnitOfWork uow)
        {
            var repo = uow.GetRepository<IOrganizationRoleRepository>();
            var items = new List<OrganizationRole>();
            foreach (var idUser in idUsers)
            {
                items.Add(new OrganizationRole
                {
                    OrganizationCode = code,
                    IdRole = idUser
                });
            }

            await repo.InsertsAsync(items);
            await uow.CommitAsync();
            return Ok();
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteAsync(
            [FromRoute] Guid code,
            [FromBody] List<Guid> idUsers,
            [FromServices] ICoreUnitOfWork uow)
        {
            var repo = uow.GetRepository<IOrganizationRoleRepository>();
            var items = new List<OrganizationRole>();
            foreach (var idUser in idUsers)
            {
                items.Add(new OrganizationRole
                {
                    OrganizationCode = code,
                    IdRole = idUser
                });
            }

            repo.Deletes(items);
            await uow.CommitAsync();
            return Ok();
        }
    }
}
