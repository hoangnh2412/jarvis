using Infrastructure.Database.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Jarvis.Core.Database;
using System;
using Jarvis.Core.Services;
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
        private readonly ICoreUnitOfWork _uow;
        private readonly IWorkContext _workcontext;

        public OrganizationRolesController(
            ICoreUnitOfWork uow,
            IWorkContext workcontext)
        {
            _uow = uow;
            _workcontext = workcontext;
        }

        [HttpGet]
        public IActionResult GetUsers([FromRoute]Guid code, [FromQuery]Paging paging)
        {
            var repo = _uow.GetRepository<IOrganizationRoleRepository>();
            var users = repo.GetRolesByOrganization(code);
            return Ok(users);
        }

        [HttpPost]
        public async Task<IActionResult> PostUsersAsync([FromRoute]Guid code, [FromBody]List<Guid> idUsers)
        {
            var repo = _uow.GetRepository<IOrganizationRoleRepository>();
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
            await _uow.CommitAsync();
            return Ok();
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteAsync([FromRoute]Guid code, [FromBody]List<Guid> idUsers)
        {
            var repo = _uow.GetRepository<IOrganizationRoleRepository>();
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
            await _uow.CommitAsync();
            return Ok();
        }
    }
}
