using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Jarvis.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Sample.DataStorage;

namespace Sample.Controllers;

[ApiController]
[Route("dapper")]
public class DapperMultitenancyController : ControllerBase
{
    private readonly IServiceProvider _serviceProvider;

    public DapperMultitenancyController(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    [HttpGet("single")]
    public async Task<IActionResult> SingleConnectionAsync(
        [FromServices] ITenantUnitOfWork uow
    )
    {
        await Task.Yield();
        // var repo = uow.GetRepository<IRepository<Tenant>>();
        // var tenants = await repo.GetQuery().ToListAsync();
        // return Ok(tenants);
        return Ok();
    }

    [HttpGet("multiple")]
    public async Task<IActionResult> MultipleConnectionAsync(
        [FromServices] ISampleUnitOfWork uow
    )
    {
        var users = await GetUsersInternal(uow);

        var tasks = new List<Task>();
        for (int i = 0; i < 5; i++)
        {
            tasks.Add(GetUsers());
        }
        await Task.WhenAll(tasks);

        return Ok(users);
    }

    [HttpGet("switch")]
    public async Task<IActionResult> SwitchConnectionAsync(
        [FromServices] IServiceProvider serviceProvider
    )
    {
        await Task.Yield();
        var result = new Dictionary<string, KeyValuePair<string, List<User>>>();

        // using (var scope = _serviceProvider.CreateScope())
        // {
        //     var uow = scope.ServiceProvider.GetService<ISampleUnitOfWork>();
        //     var dbContext = uow.GetDbContext() as DbContext;

        //     var conn = dbContext.Database.GetConnectionString();
        //     var users = await GetUsersInternal(uow);

        //     result.Add("BeforeChange", new KeyValuePair<string, List<User>>(conn, users));
        // }

        // using (var scope = _serviceProvider.CreateScope())
        // {
        //     var uow = scope.ServiceProvider.GetService<ISampleUnitOfWork>();
        //     var dbContext = uow.GetDbContext<StorageConnectionStringResolver>("tenant2") as DbContext;
        //     var conn = dbContext.Database.GetConnectionString();
        //     var users = await GetUsersInternal(uow);

        //     result.Add("Change", new KeyValuePair<string, List<User>>(conn, users));
        // }

        // using (var scope = _serviceProvider.CreateScope())
        // {
        //     var uow = scope.ServiceProvider.GetService<ISampleUnitOfWork>();
        //     var dbContext = uow.GetDbContext() as DbContext;

        //     var conn = dbContext.Database.GetConnectionString();
        //     var users = await GetUsersInternal(uow);

        //     result.Add("AfterChange", new KeyValuePair<string, List<User>>(conn, users));
        // }

        return Ok(result);
    }

    private async Task<List<User>> GetUsers()
    {
        using (var scope = _serviceProvider.CreateScope())
        {
            var uow = scope.ServiceProvider.GetService<ISampleUnitOfWork>();
            List<User> users = await GetUsersInternal(uow);
            return users;
        }
    }

    private static async Task<List<User>> GetUsersInternal(ISampleUnitOfWork uow)
    {
        await Task.Yield();
        // var repo = uow.GetRepository<IRepository<User>>();
        // var users = await repo.GetQuery().ToListAsync();
        // return users;

        return null;
    }
}