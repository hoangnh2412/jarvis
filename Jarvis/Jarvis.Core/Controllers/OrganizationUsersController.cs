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
using System.Linq;
using Jarvis.Core.Models;
using Microsoft.EntityFrameworkCore;

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
        public async Task<IActionResult> GetAsync([FromRoute] Guid code)
        {
            var repoOrganizationUser = _uow.GetRepository<IOrganizationUserRepository>();
            var users = (await repoOrganizationUser.GetUsersByOrganizationAsync(code)).Select(x => (OrganizationUserModel)x);
            return Ok(users);
        }

        [HttpPost("{idUser}/{level}")]
        public async Task<IActionResult> PostAsync([FromRoute] Guid code, [FromRoute] Guid idUser, [FromRoute] int level)
        {
            var repo = _uow.GetRepository<IOrganizationUserRepository>();
            await repo.InsertAsync(new OrganizationUser
            {
                OrganizationCode = code,
                IdUser = idUser,
                Level = level
            });
            await _uow.CommitAsync();
            return Ok();
        }

        [HttpDelete("{idUser}")]
        public async Task<IActionResult> DeleteAsync([FromRoute] Guid code, [FromRoute] Guid idUser)
        {
            var repo = _uow.GetRepository<IOrganizationUserRepository>();
            var user = await repo.GetQuery().FirstOrDefaultAsync(x => x.OrganizationCode == code && x.IdUser == idUser);
            if (user == null)
                return NotFound();

            repo.Delete(user);
            await _uow.CommitAsync();
            return Ok();
        }
    }
}
