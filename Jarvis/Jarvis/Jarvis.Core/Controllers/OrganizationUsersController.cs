using Infrastructure.Database.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using Jarvis.Core.Services;
using Jarvis.Core.Database.Poco;
using System.Collections.Generic;
using Jarvis.Core.Database;
using Jarvis.Core.Permissions;
using Jarvis.Core.Database.Repositories;
using System.Threading.Tasks;

namespace Jarvis.Core.Controllers
{
    [Authorize(nameof(CorePolicy.OrganizationPolicy.OrganizationUnit_Users))]
    [Route("organizations/{code}/users")]
    [ApiController]
    public class OrganizationUsersController : ControllerBase
    {
        private readonly ICoreUnitOfWork _uow;
        private readonly IWorkContext _workcontext;

        public OrganizationUsersController(
            ICoreUnitOfWork uow,
            IWorkContext workcontext)
        {
            _uow = uow;
            _workcontext = workcontext;
        }

        [HttpGet]
        public IActionResult Get([FromRoute]Guid code, [FromQuery]Paging paging)
        {
            var repo = _uow.GetRepository<IOrganizationUserRepository>();
            var users = repo.GetUsersByOrganizationAsync(code);
            return Ok(users);
        }

        [HttpPost]
        public async Task<IActionResult> PostAsync([FromRoute]Guid code, [FromBody]List<Guid> idUsers)
        {
            var repo = _uow.GetRepository<IOrganizationUserRepository>();
            var items = new List<OrganizationUser>();
            foreach (var idUser in idUsers)
            {
                items.Add(new OrganizationUser
                {
                    OrganizationCode = code,
                    IdUser = idUser
                });
            }

            await repo.InsertsAsync(items);
            await _uow.CommitAsync();
            return Ok();
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteAsync([FromRoute]Guid code, [FromBody]List<Guid> idUsers)
        {
            var repo = _uow.GetRepository<IOrganizationUserRepository>();
            var items = new List<OrganizationUser>();
            foreach (var idUser in idUsers)
            {
                items.Add(new OrganizationUser
                {
                    OrganizationCode = code,
                    IdUser = idUser
                });
            }

            repo.Deletes(items);
            await _uow.CommitAsync();
            return Ok();
        }
    }
}
