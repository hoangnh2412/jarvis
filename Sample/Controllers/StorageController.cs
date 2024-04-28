using Jarvis.Application.Interfaces;
using Jarvis.Application.Interfaces.Repositories;
using Jarvis.Persistence.DataContexts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sample.DataStorage;

namespace Sample.Controllers;

[ApiController]
[Route("storage")]
public class StorageController : ControllerBase
{
    private readonly IServiceProvider _serviceProvider;

    public StorageController(IServiceProvider serviceProvider)
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
    public async Task<IActionResult> SwitchConnectionAsync(
        [FromServices] IServiceProvider serviceProvider
    )
    {
        var result = new Dictionary<string, KeyValuePair<string, List<User>>>();

        using (var scope = _serviceProvider.CreateScope())
        {
            var uow = scope.ServiceProvider.GetService<ISampleUnitOfWork>();
            var dbContext = uow.GetDbContext() as DbContext;

            var conn = dbContext.Database.GetConnectionString();
            var users = await GetUsersInternal(uow);

            result.Add("BeforeChange", new KeyValuePair<string, List<User>>(conn, users));
        }

        using (var scope = _serviceProvider.CreateScope())
        {
            var uow = scope.ServiceProvider.GetService<ISampleUnitOfWork>();
            var dbContext = uow.GetDbContext("tenant2") as DbContext;
            var conn = dbContext.Database.GetConnectionString();
            var users = await GetUsersInternal(uow);

            result.Add("Change", new KeyValuePair<string, List<User>>(conn, users));
        }

        using (var scope = _serviceProvider.CreateScope())
        {
            var uow = scope.ServiceProvider.GetService<ISampleUnitOfWork>();
            var dbContext = uow.GetDbContext() as DbContext;

            var conn = dbContext.Database.GetConnectionString();
            var users = await GetUsersInternal(uow);

            result.Add("AfterChange", new KeyValuePair<string, List<User>>(conn, users));
        }
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
        var repo = uow.GetRepository<IRepository<User>>();
        var users = await repo.GetQuery().ToListAsync();
        return users;
    }
}