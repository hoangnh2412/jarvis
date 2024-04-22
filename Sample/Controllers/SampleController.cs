using Jarvis.Application.Interfaces;
using Jarvis.Application.Interfaces.Repositories;
using Jarvis.Persistence.DataContexts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Sample.DataStorage;

namespace Sample.Controllers;

[ApiController]
[Route("sample")]
public class SampleController : ControllerBase
{
    private readonly IServiceProvider _serviceProvider;

    public SampleController(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

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
        [FromServices] ISampleUnitOfWork uow,
        [FromServices] IWorkContext workContext
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
    public async Task<IActionResult> SwitchConnectionAsync()
    {
        using (var scope = _serviceProvider.CreateScope())
        {
            var uow = scope.ServiceProvider.GetService<ISampleUnitOfWork>();
            var dbContext = uow.GetDbContext() as DbContext;

            var conn = dbContext.Database.GetConnectionString();
            var users = await GetUsersInternal(uow);
            Console.WriteLine($"{conn} Users: {JsonConvert.SerializeObject(users)}");
        }

        using (var scope = _serviceProvider.CreateScope())
        {
            var uow = scope.ServiceProvider.GetService<ISampleUnitOfWork>();
            var dbContext = uow.GetDbContext("tenant2") as DbContext;
            var conn = dbContext.Database.GetConnectionString();
            var users = await GetUsersInternal(uow);
            Console.WriteLine($"{conn} Users: {JsonConvert.SerializeObject(users)}");
        }

        using (var scope = _serviceProvider.CreateScope())
        {
            var uow = scope.ServiceProvider.GetService<ISampleUnitOfWork>();
            var dbContext = uow.GetDbContext() as DbContext;

            var conn = dbContext.Database.GetConnectionString();
            var users = await GetUsersInternal(uow);
            Console.WriteLine($"{conn} Users: {JsonConvert.SerializeObject(users)}");
        }
        return Ok();
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
        var repo = uow.GetRepository<IRepository<User>>();
        var users = await repo.GetQuery().ToListAsync();
        return users;
    }
}