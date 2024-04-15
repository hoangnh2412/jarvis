using Jarvis.Application.Interfaces.Repositories;
using Jarvis.Persistence.DataContexts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sample.DataStorage;

namespace Sample.Controllers;

[ApiController]
[Route("sample")]
public class SampleController : ControllerBase
{
    [HttpGet("single")]
    public async Task<IActionResult> SingleConnectionAsync(
        [FromServices] ITenantUnitOfWork uow
    )
    {
        var repo = uow.GetRepository<IRepository<Tenant>>();
        var tenants = await repo.GetQuery().ToListAsync();
        return Ok(tenants);
    }

    [HttpGet("multiple")]
    public async Task<IActionResult> MultipleConnectionAsync(
        [FromServices] ISampleUnitOfWork uow
    )
    {
        var repo = uow.GetRepository<IRepository<User>>();
        var users = await repo.GetQuery().ToListAsync();
        return Ok(users);
    }
}