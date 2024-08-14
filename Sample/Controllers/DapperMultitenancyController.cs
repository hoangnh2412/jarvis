using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sample.DataStorage;
using Sample.DataStorage.EntityFramework;

namespace Sample.Controllers;

[ApiController]
[Route("dapper")]
public class DapperMultitenancyController : ControllerBase
{
    [HttpGet("single")]
    public async Task<IActionResult> SingleConnectionAsync(
        [FromServices] ITenantUnitOfWork uow
    )
    {
        var items = await uow.GetConnection().QueryAsync<Tenant>("SELECT * FROM tenants");
        return Ok(items);
    }

    [HttpGet("multiple")]
    public async Task<IActionResult> MultipleConnectionAsync(
        [FromServices] ISampleUnitOfWork uow,
        [FromServices] IServiceScopeFactory scopeFactory
    )
    {
        var users = await GetUsers(uow);

        var tasks = new List<Task>();
        for (int i = 0; i < 5; i++)
        {
            tasks.Add(Task.Run(async () =>
            {
                using (var scope = scopeFactory.CreateScope())
                {
                    var scopeUoW = scope.ServiceProvider.GetService<ISampleUnitOfWork>();
                    return await GetUsers(scopeUoW);
                }
            }));
        }
        await Task.WhenAll(tasks);

        return Ok(users);
    }

    [HttpGet("switch")]
    public async Task<IActionResult> SwitchConnectionAsync(
        [FromServices] IServiceScopeFactory scopeFactory
    )
    {
        var result = new Dictionary<string, KeyValuePair<string, List<User>>>();

        using (var scope = scopeFactory.CreateScope())
        {
            var uow = scope.ServiceProvider.GetService<ISampleUnitOfWork>();
            var conn = uow.GetConnectionString();
            var users = await GetUsers(uow);

            result.Add("BeforeChange", new KeyValuePair<string, List<User>>(conn, users.ToList()));
        }

        using (var scope = scopeFactory.CreateScope())
        {
            var uow = scope.ServiceProvider.GetService<ISampleUnitOfWork>();
            var dbContext = uow.GetDbContext<StorageConnectionStringResolver>("tenant2") as DbContext;
            var conn = dbContext.Database.GetConnectionString();
            var users = await GetUsers(uow);

            result.Add("Change", new KeyValuePair<string, List<User>>(conn, users.ToList()));
        }

        using (var scope = scopeFactory.CreateScope())
        {
            var uow = scope.ServiceProvider.GetService<ISampleUnitOfWork>();
            var conn = uow.GetConnectionString();
            var users = await GetUsers(uow);

            result.Add("AfterChange", new KeyValuePair<string, List<User>>(conn, users.ToList()));
        }

        return Ok(result);
    }

    private async Task<IEnumerable<User>> GetUsers(ISampleUnitOfWork uow)
    {
        return await uow.GetConnection().QueryAsync<User>("SELECT * FROM users");
    }
}