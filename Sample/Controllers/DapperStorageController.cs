using System.Linq.Expressions;
using Dapper;
using Jarvis.Application.Interfaces.Repositories;
using Jarvis.Persistence.Repositories.Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sample.DataStorage;

using DapperExtensions;
using Dapper.Contrib.Linq2Dapper;

namespace Sample.Controllers;

[ApiController]
[Route("dapper")]
public class DapperStorageController : ControllerBase
{
    private readonly IServiceProvider _serviceProvider;

    public DapperStorageController(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    [HttpGet("read")]
    public async Task<IActionResult> ReadAsync(
        [FromServices] ITenantUnitOfWork uow
    )
    {
        var repo = uow.GetRepository<IRepository<Tenant>>();
        var dbContext = uow.GetDbContext() as DapperDbContext;
        var connection = dbContext.Connection;
        var items = await connection.QueryAsync<Tenant>("SELECT * FROM tenants");
        return Ok("OK");
    }

    [HttpGet("create")]
    public async Task<IActionResult> CreateAsync(
        [FromServices] ISampleUnitOfWork uow
    )
    {
        var repo = uow.GetRepository<IRepository<User>>();
        var users = await repo.InsertAsync(new DataStorage.User
        {
            Name = "sample",
            Age = 11
        });
        await uow.CommitAsync();
        return Ok(users);
    }

    [HttpGet("update")]
    public async Task<IActionResult> UpdateAsync(
        [FromServices] ISampleUnitOfWork uow
    )
    {
        var repo = uow.GetRepository<IRepository<User>>();
        var user = await repo.GetQuery().FirstOrDefaultAsync(x => x.Id == 1);

        user.Name = "updated";
        user.Age = 12;

        await repo.UpdateAsync(user);
        await uow.CommitAsync();
        return Ok(user);
    }

    [HttpGet("delete")]
    public async Task<IActionResult> DeleteAsync(
        [FromServices] ISampleUnitOfWork uow
    )
    {
        var repo = uow.GetRepository<IRepository<User>>();
        var user = await repo.GetQuery().FirstOrDefaultAsync(x => x.Id == 2);

        await repo.DeleteAsync(user);
        await uow.CommitAsync();
        return Ok(user);
    }
}