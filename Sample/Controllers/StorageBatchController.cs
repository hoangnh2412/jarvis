using Jarvis.Application.Interfaces.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sample.DataStorage;

namespace Sample.Controllers;

[ApiController]
[Route("storage/batch")]
public class StorageBatchController : ControllerBase
{
    private readonly IServiceProvider _serviceProvider;

    public StorageBatchController(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    [HttpGet("create")]
    public async Task<IActionResult> CreateAsync(
        [FromServices] ISampleUnitOfWork uow
    )
    {
        var repo = uow.GetRepository<IRepository<User>>();

        var users = new List<User>();
        for (int i = 0; i < 100; i++)
        {
            var random = new Random();
            users.Add(new DataStorage.User
            {
                Age = random.Next(1, 50),
                Name = Guid.NewGuid().ToString("N").ToUpper()
            });
        }

        await repo.InsertBatchAsync(users);
        return Ok(users);
    }

    [HttpGet("update")]
    public async Task<IActionResult> UpdateSelectAsync(
        [FromServices] ISampleUnitOfWork uow
    )
    {
        var ids = new int[] { 3, 4, 5, 6, 7 };
        var repo = uow.GetRepository<IRepository<User>>();

        var users = await repo.GetQuery().Where(x => ids.Contains(x.Id)).ToListAsync();
        await repo.UpdateBatchAsync(users, x => new DataStorage.User
        {
            Name = "Updated 66",
            Age = 66
        });
        return Ok(users);
    }

    [HttpGet("update-without-select")]
    public async Task<IActionResult> UpdateWithoutSelectAsync(
        [FromServices] ISampleUnitOfWork uow
    )
    {
        var ids = new int[] { 3, 4, 5, 6, 7 };
        var repo = uow.GetRepository<IRepository<User>>();

        var queryable = repo.GetQuery().Where(x => ids.Contains(x.Id) && x.Age < 100);
        var result = await repo.UpdateBatchAsync(queryable, x => new DataStorage.User
        {
            Name = "Updated 3",
            Age = 33
        });

        return Ok(result);
    }

    [HttpGet("delete")]
    public async Task<IActionResult> DeleteSelectAsync(
        [FromServices] ISampleUnitOfWork uow
    )
    {
        var ids = new int[] { 8, 9 };
        var repo = uow.GetRepository<IRepository<User>>();

        var users = await repo.GetQuery().Where(x => ids.Contains(x.Id)).ToListAsync();
        await repo.DeleteBatchAsync(users);
        return Ok(users);
    }

    [HttpGet("delete-without-select")]
    public async Task<IActionResult> DeleteWithoutSelectAsync(
        [FromServices] ISampleUnitOfWork uow
    )
    {
        var ids = new int[] { 10, 11 };
        var repo = uow.GetRepository<IRepository<User>>();

        var queryable = repo.GetQuery().Where(x => ids.Contains(x.Id));
        var result = await repo.DeleteBatchAsync(queryable);
        return Ok(result);
    }
}