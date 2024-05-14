using Jarvis.Application.Interfaces.Repositories;
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

    [HttpGet("read")]
    public async Task<IActionResult> ReadAsync(
        [FromServices] ISampleUnitOfWork uow
    )
    {
        var repo = uow.GetRepository<IRepository<User>>();
        var users = await repo.GetQuery().ToListAsync();
        return Ok(users);
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
}